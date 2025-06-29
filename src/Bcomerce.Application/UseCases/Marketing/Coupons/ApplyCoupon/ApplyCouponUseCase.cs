using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Orders;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Marketing.Coupons.Repositories;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;

public class ApplyCouponUseCase : IApplyCouponUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IOrderRepository _orderRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _uow;

    public ApplyCouponUseCase(ILoggedUser loggedUser, IOrderRepository orderRepository, ICouponRepository couponRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _orderRepository = orderRepository;
        _couponRepository = couponRepository;
        _uow = uow;
    }

    public async Task<Result<OrderOutput, Notification>> Execute(ApplyCouponInput input)
    {
        var notification = Notification.Create();
        var clientId = _loggedUser.GetClientId();

        // O OrderRepository.Get precisaria ser implementado para buscar o pedido
        // Por agora, vamos assumir que o método existe e focar no fluxo.
        // Em um passo futuro, implementaríamos o Get com hidratação completa.
        var order = await _orderRepository.Get(input.OrderId, CancellationToken.None);

        if (order is null || order.ClientId != clientId)
        {
            notification.Append(new Error("Pedido não encontrado."));
            return Result<OrderOutput, Notification>.Fail(notification);
        }

        var coupon = await _couponRepository.GetByCodeAsync(input.CouponCode, CancellationToken.None);
        if (coupon is null)
        {
            notification.Append(new Error("Cupom inválido."));
            return Result<OrderOutput, Notification>.Fail(notification);
        }

        try
        {
            order.ApplyCoupon(coupon);
        }
        catch (DomainException ex)
        {
            notification.Append(new Error(ex.Message));
            return Result<OrderOutput, Notification>.Fail(notification);
        }

        await _uow.Begin();
        try
        {
            await _orderRepository.Update(order, CancellationToken.None);
            await _couponRepository.Update(coupon, CancellationToken.None); // Salva o estado do cupom (ex: times_used++)
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Não foi possível aplicar o cupom ao pedido."));
            return Result<OrderOutput, Notification>.Fail(notification);
        }

        return Result<OrderOutput, Notification>.Ok(OrderOutput.FromOrder(order));
    }
}