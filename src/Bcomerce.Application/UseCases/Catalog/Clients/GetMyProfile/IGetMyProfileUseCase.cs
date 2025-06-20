using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Clients.Create;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.GetMyProfile;

public interface IGetMyProfileUseCase : IUseCase<object, CreateClientOutput, Notification>
{

}