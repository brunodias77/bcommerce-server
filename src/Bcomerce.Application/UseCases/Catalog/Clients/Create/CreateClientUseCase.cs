using System.Data.Common;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Create;

public class CreateClientUseCase : ICreateClientUseCase
{
    private readonly IClientRepository _clientRepository; 
    private readonly IUnitOfWork _uow;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly ILogger<CreateClientUseCase> _logger; 
    private readonly IDomainEventPublisher _publisher; 

    public CreateClientUseCase(
        IClientRepository clientRepository, 
        IUnitOfWork uow, 
        IPasswordEncripter passwordEncripter, 
        ILogger<CreateClientUseCase> logger, 
        IDomainEventPublisher publisher)
    {
        _clientRepository = clientRepository;
        _uow = uow;
        _passwordEncripter = passwordEncripter;
        _logger = logger;
        _publisher = publisher;
    }
    
    public async Task<Result<CreateClientOutput, Notification>> Execute(CreateClientInput input)
    {
        var notification = Notification.Create();
    
        if (string.IsNullOrWhiteSpace(input.Password))
        {
            notification.Append(new Error("A senha não pode estar vazia."));
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }

        var emailExists = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
        if (emailExists != null)
        {
            notification.Append(new Error("O e-mail informado já está em uso."));
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }

        var passwordHash = _passwordEncripter.Encrypt(input.Password);
        var client = Client.NewClient(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            passwordHash,
            cpf: null, 
            dateOfBirth: null,
            input.NewsletterOptIn,
            notification
        );
    
        if (notification.HasError())
        {
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }
    
        await _uow.Begin();
        try
        {
            await _clientRepository.Insert(client, CancellationToken.None);
            await _uow.Commit();
            
            foreach (var domainEvent in client.Events)
            {
                await _publisher.PublishAsync((dynamic)domainEvent, CancellationToken.None);
            }

            return Result<CreateClientOutput, Notification>.Ok(CreateClientOutput.FromClient(client));
        }
        catch (Exception e)
        {
            if (_uow.HasActiveTransaction)
            {
                await _uow.Rollback();
            }

            _logger.LogError(e, "Ocorreu um erro inesperado ao tentar criar o cliente com e-mail {ClientEmail}", input.Email);
            
            // CORREÇÃO: Utilizando Create() e Append() para criar a notificação de erro.
            var errorNotification = (Notification)Notification.Create()
                .Append(new Error("Não foi possível processar seu registro. Tente novamente mais tarde."));
                
            return Result<CreateClientOutput, Notification>.Fail(errorNotification);
        }
    }
}


