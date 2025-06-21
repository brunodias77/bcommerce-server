using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Create;

public interface ICreateClientUseCase : IUseCase<CreateClientInput, CreateClientOutput, Notification>;
