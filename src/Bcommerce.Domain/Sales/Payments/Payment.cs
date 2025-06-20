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

    internal static Payment NewPayment(Guid orderId, Money amount, PaymentMethod method)
    {
        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending
        };
    }

    internal void MarkAsApproved(string transactionId)
    {
        DomainException.ThrowWhen(Status != PaymentStatus.Pending, "Só é possível aprovar um pagamento pendente.");
        Status = PaymentStatus.Approved;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    internal void MarkAsDeclined()
    {
        DomainException.ThrowWhen(Status != PaymentStatus.Pending, "Só é possível recusar um pagamento pendente.");
        Status = PaymentStatus.Declined;
        ProcessedAt = DateTime.UtcNow;
    }

    public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
}