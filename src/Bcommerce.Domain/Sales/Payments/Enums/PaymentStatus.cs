namespace Bcommerce.Domain.Sales.Payments.Enums;

public enum PaymentStatus
{
    Pending,
    Approved,
    Declined,
    Refunded,
    PartiallyRefunded,
    Chargeback,
    Error
}