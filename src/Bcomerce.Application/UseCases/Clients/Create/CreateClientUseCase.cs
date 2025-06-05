using System.Data.Common;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Bcomerce.Application.UseCases.Clients.Create;

public class CreateClientUseCase : ICreateClientUseCase
{
    public CreateClientUseCase(IClientRepository clientRepository, IUnitOfWork uow, IPasswordEncripter passwordEncripter)
    {
        _clientRepository = clientRepository;
        _uow = uow;
        _passwordEncripter = passwordEncripter;
    }

    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordEncripter _passwordEncripter;
    
    public async Task<Result<CreateClientOutput, Notification>> Execute(CreateClientInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Password))
        {
            return Result<CreateClientOutput, Notification>.Fail(Notification.Create(new Error("Erro desconhecido")));
        }

        string passwordHash;
        try
        {
            passwordHash =  _passwordEncripter.Encrypt(input.Password);
        }
        catch (Exception e)
        {
            return Result<CreateClientOutput, Notification>.Fail(Notification.Create(new Error("Erro desconhecido")));
        }
        
        var clientValidation = Notification.Create();
        var client = Client.NewClient(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            passwordHash,
            input.Cpf,
            input.DateOfBirth,
            input.NewsletterOptIn,
            clientValidation
        );
        
        if (clientValidation.HasError())
        {
            // Quero mandar os erros que estao no clientValidation
            Result<CreateClientOutput, Notification>.Fail(Notification.Create(new Error("Existem Erros")));
        }
        
        // Verificar se o email do client ja existe na base de dados

        try
        {
            
        }
        catch (DbException dbEx)
        {


        }
        catch (Exception e)
        {
            if (_uow.HasActiveTransaction)
            { 
                await _uow.Rollback();
            }
            return Result<CreateClientOutput, Notification>.Fail(Notification.Create(new Error("Erro desconhecido")));
        }
        return Result<CreateClientOutput, Notification>.Ok(CreateClientOutput.FromClient(client));
    }
}


