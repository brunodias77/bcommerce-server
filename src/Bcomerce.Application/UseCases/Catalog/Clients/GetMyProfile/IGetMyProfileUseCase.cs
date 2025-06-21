using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcommerce.Domain.Validation.Handlers;


namespace Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;

public interface IGetMyProfileUseCase : IUseCase<object, CreateClientOutput, Notification>
{

}