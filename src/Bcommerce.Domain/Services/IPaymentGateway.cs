// Representa o resultado de uma tentativa de processamento de pagamento.
using Bcommerce.Domain.Sales.Orders;

namespace Bcommerce.Domain.Services;

public record PaymentGatewayResult(bool IsSuccess, string? TransactionId, string? FailureReason);

// Contrato que qualquer gateway de pagamento deve seguir.
public interface IPaymentGateway
{
    /// <summary>
    /// Processa o pagamento para um determinado pedido.
    /// </summary>
    /// <param name="order">O pedido a ser pago.</param>
    /// <param name="paymentMethodToken">Um token que representa o método de pagamento (ex: token de cartão de crédito).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O resultado da transação do gateway.</returns>
    Task<PaymentGatewayResult> ProcessPaymentAsync(Order order, string paymentMethodToken, CancellationToken cancellationToken);
}