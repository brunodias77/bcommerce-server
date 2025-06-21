using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;

public interface ILoginClientUseCase: IUseCase<LoginClientInput, LoginClientOutput, Notification>
{
    
}