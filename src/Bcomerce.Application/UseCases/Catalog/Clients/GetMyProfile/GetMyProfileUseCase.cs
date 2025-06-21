using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;


namespace Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;

public class GetMyProfileUseCase : IGetMyProfileUseCase
{
    public GetMyProfileUseCase(IClientRepository clientRepository, ILoggedUser loggedUser)
    {
        _clientRepository = clientRepository;
        _loggedUser = loggedUser;
    }

    private readonly IClientRepository _clientRepository;
    private readonly ILoggedUser _loggedUser;
    public async Task<Result<CreateClientOutput, Notification>> Execute(object input)
    {
        var clientId = _loggedUser.GetClientId();
        var client = await _clientRepository.Get(clientId, CancellationToken.None);

        if (client is null)
        {
            // CORREÇÃO: Utilizando Create() e Append() para criar a notificação de erro.
            var notification = Notification.Create()
                .Append(new Error("Usuário não encontrado."));
            return Result<CreateClientOutput, Notification>.Fail(notification);
        }
        
        var output = CreateClientOutput.FromClient(client);
        return Result<CreateClientOutput, Notification>.Ok(output);    
    }
}