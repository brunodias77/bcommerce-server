using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Infrastructure.Services;

/// <summary>
/// Uma implementação falsa de um gateway de pagamento para fins de desenvolvimento e teste.
/// Simula respostas de sucesso ou falha com base no token do método de pagamento.
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
    private readonly ILogger<FakePaymentGateway> _logger;

    public FakePaymentGateway(ILogger<FakePaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentGatewayResult> ProcessPaymentAsync(Order order, string paymentMethodToken, CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- FAKE PAYMENT GATEWAY: Processando pagamento para o pedido {OrderRef} ---", order.ReferenceCode);
        _logger.LogInformation("Valor: {Amount}", order.GrandTotalAmount);
        _logger.LogInformation("Token do Método de Pagamento: {Token}", paymentMethodToken);

        // Lógica de simulação:
        // Se o token for "approved-token", a transação é um sucesso.
        // Qualquer outro token resultará em falha.
        if (paymentMethodToken == "approved-token")
        {
            var transactionId = $"fake_txn_{Guid.NewGuid()}";
            _logger.LogInformation("Resultado: SUCESSO. ID da Transação: {TxnId}", transactionId);
            var result = new PaymentGatewayResult(true, transactionId, null);
            return Task.FromResult(result);
        }
        else
        {
            var reason = "Cartão recusado pelo emissor.";
            _logger.LogWarning("Resultado: FALHA. Motivo: {Reason}", reason);
            var result = new PaymentGatewayResult(false, null, reason);
            return Task.FromResult(result);
        }
    }


}