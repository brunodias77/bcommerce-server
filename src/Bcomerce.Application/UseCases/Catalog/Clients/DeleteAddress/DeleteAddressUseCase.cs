using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace  Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;

public class DeleteAddressUseCase : IDeleteAddressUseCase
{
    public DeleteAddressUseCase(ILoggedUser loggedUser, IAddressRepository addressRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _addressRepository = addressRepository;
        _uow = uow;
    }

    private readonly ILoggedUser _loggedUser;
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _uow;
    public async Task<Result<bool, Notification>> Execute(DeleteAddressInput input)
    {
        var clientId = _loggedUser.GetClientId();
        var notification = Notification.Create();

        await _uow.Begin();
        try
        {
            // O addressId agora vem do objeto de input
            var address = await _addressRepository.GetByIdAsync(input.AddressId, CancellationToken.None);

            // PASSO DE SEGURANÇA CRUCIAL
            if (address is null || address.ClientId != clientId)
            {
                notification.Append(new Error("Endereço não encontrado."));
                await _uow.Rollback();
                return Result<bool, Notification>.Fail(notification);
            }

            address.SoftDelete();

            await _addressRepository.UpdateAsync(address, CancellationToken.None);
            await _uow.Commit();

            return Result<bool, Notification>.Ok(true);
        }
        catch (Exception e)
        {
            // Logar o erro 'e'
            await _uow.Rollback();
            notification.Append(new Error("Erro ao remover o endereço."));
            return Result<bool, Notification>.Fail(notification);
        }    
    }
}