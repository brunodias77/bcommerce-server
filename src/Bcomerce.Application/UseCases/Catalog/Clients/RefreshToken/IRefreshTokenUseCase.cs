using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.RefreshToken;

public interface IRefreshTokenUseCase : IUseCase<RefreshTokenInput, LoginClientOutput, Notification>
{
}