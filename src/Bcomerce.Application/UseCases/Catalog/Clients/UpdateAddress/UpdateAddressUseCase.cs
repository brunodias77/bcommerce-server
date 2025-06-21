using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Clients.UpdateAddress;

public class UpdateAddressUseCase : IUpdateAddressUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _uow;

    public UpdateAddressUseCase(ILoggedUser loggedUser, IAddressRepository addressRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _addressRepository = addressRepository;
        _uow = uow;
    }
    public async Task<Result<AddressOutput, Notification>> Execute(UpdateAddressInput input)
    {
        var clientId = _loggedUser.GetClientId();
        var notification = Notification.Create();
        await _uow.Begin();
        try
        {
            // O addressId agora vem do objeto de input
            var address = await _addressRepository.GetByIdAsync(input.AddressId, CancellationToken.None);

            if (address is null || address.ClientId != clientId)
            {
                notification.Append(new Error("Endereço não encontrado."));
                await _uow.Rollback();
                return Result<AddressOutput, Notification>.Fail(notification);
            }

            address.Update(
                input.Type, input.PostalCode, input.Street, input.StreetNumber, input.Complement,
                input.Neighborhood, input.City, input.StateCode, input.IsDefault, notification
            );

            if (notification.HasError())
            {
                await _uow.Rollback();
                return Result<AddressOutput, Notification>.Fail(notification);
            }

            await _addressRepository.UpdateAsync(address, CancellationToken.None);
            await _uow.Commit();
            return Result<AddressOutput, Notification>.Ok(AddressOutput.FromAddress(address));
        }
        catch (Exception e)
        {
            await _uow.Rollback();
            notification.Append(new Error("Erro ao atualizar o endereço."));
            return Result<AddressOutput, Notification>.Fail(notification);
        }
    }
}