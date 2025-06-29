using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Payments.Enums;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Payments;

public class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Money Amount { get; private set; }
    public string? TransactionId { get; private set; } // ID do gateway de pagamento
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment NewPayment(Guid orderId, Money amount, PaymentMethod method)
    {
        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending
        };
    }

    public void MarkAsApproved(string transactionId)
    {
        DomainException.ThrowWhen(Status != PaymentStatus.Pending, "Só é possível aprovar um pagamento pendente.");
        Status = PaymentStatus.Approved;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsDeclined()
    {
        DomainException.ThrowWhen(Status != PaymentStatus.Pending, "Só é possível recusar um pagamento pendente.");
        Status = PaymentStatus.Declined;
        ProcessedAt = DateTime.UtcNow;
    }

    // --- NOVO MÉTODO ADICIONADO ---
    public void MarkAsRefunded()
    {
        // Regra de negócio: Só pode reembolsar um pagamento que foi aprovado.
        DomainException.ThrowWhen(Status != PaymentStatus.Approved, "Somente pagamentos aprovados podem ser reembolsados.");
        Status = PaymentStatus.Refunded;
        ProcessedAt = DateTime.UtcNow; // Atualiza a data do último processamento
    }

    public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
}