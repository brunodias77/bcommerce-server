using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Sales.Payments.Enums;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Orders.ProcessPayment
{
    public class ProcessPaymentUseCase : IProcessPaymentUseCase
    {
        private readonly ILoggedUser _loggedUser;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _uow;

        public ProcessPaymentUseCase(ILoggedUser loggedUser, IOrderRepository orderRepository, IPaymentGateway paymentGateway, IUnitOfWork uow)
        {
            _loggedUser = loggedUser;
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _uow = uow;
        }

        public async Task<Result<OrderOutput, Notification>> Execute(ProcessPaymentInput input)
        {
            var notification = Notification.Create();
            var clientId = _loggedUser.GetClientId();
            var order = await _orderRepository.Get(input.OrderId, CancellationToken.None);

            if (order is null || order.ClientId != clientId)
            {
                notification.Append(new Error("Pedido não encontrado."));
                return Result<OrderOutput, Notification>.Fail(notification);
            }

            // Adiciona um item de pagamento ao pedido antes de processar
            order.AddPayment(PaymentMethod.CreditCard); // Assumindo cartão de crédito

            // Chama o gateway de pagamento
            var gatewayResult = await _paymentGateway.ProcessPaymentAsync(order, input.PaymentMethodToken, CancellationToken.None);

            if (!gatewayResult.IsSuccess)
            {
                notification.Append(new Error(gatewayResult.FailureReason ?? "Pagamento recusado."));
                return Result<OrderOutput, Notification>.Fail(notification);
            }

            // Se o pagamento foi um sucesso, confirma o pagamento no agregado
            var payment = order.Payments.First(); // Pega o pagamento recém-adicionado
            order.ConfirmPayment(payment.Id, gatewayResult.TransactionId!);

            await _uow.Begin();
            try
            {
                // O OrderRepository.Update precisa ser capaz de salvar o novo pagamento também
                await _orderRepository.Update(order, CancellationToken.None);
                await _uow.Commit();
            }
            catch (Exception)
            {
                await _uow.Rollback();
                notification.Append(new Error("Ocorreu um erro ao finalizar seu pedido após o pagamento. Contate o suporte."));
                return Result<OrderOutput, Notification>.Fail(notification);
            }

            return Result<OrderOutput, Notification>.Ok(OrderOutput.FromOrder(order));
        }
    }
}