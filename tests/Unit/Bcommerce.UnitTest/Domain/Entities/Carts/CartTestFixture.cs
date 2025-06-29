using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Carts
{
    [CollectionDefinition(nameof(CartTestFixture))]
    public class CartTestFixtureCollection : ICollectionFixture<CartTestFixture> { }

    public class CartTestFixture
    {
        public Faker Faker { get; }

        public CartTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public Cart CreateValidCart(Guid? clientId = null)
        {
            return Cart.NewCart(clientId ?? Guid.NewGuid());
        }

        public (Guid ProductVariantId, int Quantity, Money Price) GetValidCartItemInput()
        {
            return (
                Guid.NewGuid(),
                Faker.Random.Int(1, 5),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 100)))
            );
        }
    }
}