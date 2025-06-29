using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Sales.Orders;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using Bcommerce.Domain.Validation.Handlers;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Orders
{
    [CollectionDefinition(nameof(OrderTestFixture))]
    // CORREÇÃO: A interface genérica foi simplificada para referenciar a classe da fixture diretamente.
    public class OrderTestFixtureCollection : ICollectionFixture<OrderTestFixture> { }

    public class OrderTestFixture
    {
        public Faker Faker { get; }

        public OrderTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        // --- MÉTODOS DE APOIO PARA CRIAÇÃO DE DADOS DE TESTE ---

        public Client CreateValidClient() =>
            Client.NewClient(
                Faker.Name.FirstName(),
                Faker.Name.LastName(),
                Faker.Internet.Email(),
                Faker.Phone.PhoneNumber(),
                "valid_password_hash", null, null, false, Notification.Create()
            );

        public Address CreateValidAddress(Guid clientId, AddressType type = AddressType.Shipping)
        {
            return Address.NewAddress(
                clientId, type, Faker.Address.ZipCode().Replace("-", ""), Faker.Address.StreetName(),
                Faker.Random.Number(100, 999).ToString(), Faker.Address.SecondaryAddress(),
                Faker.Address.StreetName(), // Usando StreetName como aproximação para bairro
                Faker.Address.City(), Faker.Address.StateAbbr(), true, Notification.Create()
            );
        }
        
        public List<CartItem> CreateValidCartItems(Guid cartId, int count = 2)
        {
            var items = new List<CartItem>();
            for (int i = 0; i < count; i++)
            {
                items.Add(CartItem.NewItem(
                    cartId, 
                    Guid.NewGuid(), 
                    Faker.Random.Int(1, 3), 
                    Money.Create(decimal.Parse(Faker.Commerce.Price(50, 200)))
                ));
            }
            return items;
        }

        /// <summary>
        /// NOVO MÉTODO: Centraliza a criação de um pedido válido para ser usado nos testes.
        /// </summary>
        public Order CreateValidOrder()
        {
            var client = CreateValidClient();
            var shippingAddress = CreateValidAddress(client.Id);
            var billingAddress = CreateValidAddress(client.Id, AddressType.Billing);
            var cartItems = CreateValidCartItems(Guid.NewGuid());
            var shippingAmount = Money.Create(Faker.Random.Decimal(10, 30));

            return Order.NewOrderFromCart(client, cartItems, shippingAmount, shippingAddress, billingAddress);
        }
    }
}