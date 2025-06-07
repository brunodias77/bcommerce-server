using System.Data.Common;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Bcomerce.Application.UseCases.Clients.Create;

public class CreateClientUseCase : ICreateClientUseCase
{
    public CreateClientUseCase(IClientRepository clientRepository, IUnitOfWork uow, IPasswordEncripter passwordEncripter, ILogger<CreateClientUseCase> logger, IDomainEventPublisher publisher)
    {
        _clientRepository = clientRepository;
        _uow = uow;
        _passwordEncripter = passwordEncripter;
        _logger = logger;
        _publisher = publisher;
    }


    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly ILogger<CreateClientUseCase> _logger; 
    private readonly IDomainEventPublisher _publisher; 
    
    public async Task<Result<CreateClientOutput, Notification>> Execute(CreateClientInput input)
    {
        var notification = Notification.Create();
        if (string.IsNullOrWhiteSpace(input.Password))
        {
            notification.Append(new Error("A senha nao pode estar vazia!"));
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }

        var passwordHash = _passwordEncripter.Encrypt(input.Password);
        var client = Client.NewClient(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            passwordHash,
            null,
            null,
            input.NewsletterOptIn,
            notification
        );
        
        // 4. Se a validação da entidade falhar, retorne os erros
        if (notification.HasError())
        {
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }
        await _uow.Begin();
        try
        {
            // 5. Verifique se o e-mail já existe DENTRO da transação
            var emailExists = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
            if (emailExists != null)
            {
                await _uow.Rollback();
                notification.Append(new Error("O e-mail informado já está em uso."));
                return Result<CreateClientOutput, Notification>.Fail(notification);
            }

            // 6. Insira o cliente no banco de dados
            await _clientRepository.Insert(client, CancellationToken.None);

            // 7. Confirme a transação
            await _uow.Commit();
            
            // Publica todos os eventos que foram levantados na entidade
            foreach (var domainEvent in client.Events)
            {
                await _publisher.PublishAsync(domainEvent, CancellationToken.None);
            }

            // 8. Retorne sucesso
            return Result<CreateClientOutput, Notification>.Ok(CreateClientOutput.FromClient(client));
        }
        catch (Exception e)
        {
            // 9. Em caso de qualquer outra exceção, reverta a transação
            if (_uow.HasActiveTransaction)
            {
                await _uow.Rollback();
            }
            _logger.LogError(e, "Ocorreu um erro inesperado ao tentar criar o cliente com e-mail {ClientEmail}", input.Email);
            var error = Notification.Create(new Error("Ocorreu um erro inesperado ao criar o cliente."));
            return Result<CreateClientOutput, Notification>.Fail(error);
        }
    }
}


