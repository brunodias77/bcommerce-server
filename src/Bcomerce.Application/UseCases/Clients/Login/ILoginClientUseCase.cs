using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.Login;

public interface ILoginClientUseCase: IUseCase<LoginClientInput, LoginClientOutput, Notification>
{
    
}