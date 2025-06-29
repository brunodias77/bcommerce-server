using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;

public interface ICreateOrderUseCase : IUseCase<CreateOrderInput, OrderOutput, Notification>
{
}