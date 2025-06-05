using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.Create;

public interface ICreateClientUseCase : IUseCase<CreateClientInput, CreateClientOutput, Notification>;
