using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;

public class CreateOrderUseCase : ICreateOrderUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IClientRepository _clientRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;

    public CreateOrderUseCase(ILoggedUser loggedUser, IClientRepository clientRepository, IAddressRepository addressRepository, ICartRepository cartRepository, IOrderRepository orderRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _clientRepository = clientRepository;
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _uow = uow;
    }

    public async Task<Result<OrderOutput, Notification>> Execute(CreateOrderInput input)
    {
        var notification = Notification.Create();
        var clientId = _loggedUser.GetClientId();

        var client = await _clientRepository.Get(clientId, CancellationToken.None);
        var cart = await _cartRepository.GetByClientIdAsync(clientId, CancellationToken.None);
        var shippingAddress = await _addressRepository.GetByIdAsync(input.ShippingAddressId, CancellationToken.None);
        var billingAddress = await _addressRepository.GetByIdAsync(input.BillingAddressId, CancellationToken.None);

        #region Validations
        if (cart is null || !cart.Items.Any())
            notification.Append(new Error("Seu carrinho está vazio."));
        if (client is null)
            notification.Append(new Error("Cliente não encontrado."));
        if (shippingAddress is null || shippingAddress.ClientId != clientId)
            notification.Append(new Error("Endereço de entrega inválido."));
        if (billingAddress is null || billingAddress.ClientId != clientId)
            notification.Append(new Error("Endereço de cobrança inválido."));

        if (notification.HasError())
            return Result<OrderOutput, Notification>.Fail(notification);
        #endregion

        // A entidade de domínio é responsável por criar o pedido a partir dos dados.
        var order = Order.NewOrderFromCart(
            client!,
            cart!.Items,
            Money.Create(input.ShippingFee),
            shippingAddress!,
            billingAddress!
        );

        // A entidade de domínio limpa o carrinho.
        cart.Clear();

        await _uow.Begin();
        try
        {
            await _orderRepository.Insert(order, CancellationToken.None);
            await _cartRepository.Update(cart, CancellationToken.None); // Persiste o carrinho (agora vazio)
            await _uow.Commit();
        }
        catch (Exception ex)
        {
            await _uow.Rollback();
            // Logar a exceção 'ex'
            notification.Append(new Error("Não foi possível finalizar seu pedido. Tente novamente."));
            return Result<OrderOutput, Notification>.Fail(notification);
        }

        return Result<OrderOutput, Notification>.Ok(OrderOutput.FromOrder(order));
    }
}