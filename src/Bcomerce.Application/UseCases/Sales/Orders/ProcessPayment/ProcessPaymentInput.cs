using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Sales.Orders.ProcessPayment
{
    public record ProcessPaymentInput(
        Guid OrderId,
        string PaymentMethodToken // Ex: "tok_visa", "approved-token", etc.
    );
}