using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Logout;

public interface ILogoutUseCase : IUseCase<object, bool, Notification> {}
