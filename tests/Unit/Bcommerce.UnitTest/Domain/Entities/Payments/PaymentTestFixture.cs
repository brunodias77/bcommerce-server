using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Sales.Payments;
using Bcommerce.Domain.Sales.Payments.Enums;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Payments
{
    [CollectionDefinition(nameof(PaymentTestFixture))]
    public class PaymentTestFixtureCollection : ICollectionFixture<PaymentTestFixture> { }

    public class PaymentTestFixture
    {
        public Faker Faker { get; }

        public PaymentTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public Payment CreateValidPendingPayment()
        {
            return Payment.NewPayment(
                Guid.NewGuid(),
                Money.Create(Faker.Random.Decimal(10, 1000)),
                Faker.PickRandom<PaymentMethod>()
            );
        }
    }
}