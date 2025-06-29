using Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Orders.CreateOrder;

[CollectionDefinition(nameof(CreateOrderUseCaseTestFixture))]
    public class CreateOrderUseCaseTestFixtureCollection : ICollectionFixture<CreateOrderUseCaseTestFixture> { }

    public class CreateOrderUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }
        public Mock<IClientRepository> ClientRepositoryMock { get; }
        public Mock<IAddressRepository> AddressRepositoryMock { get; }
        public Mock<ICartRepository> CartRepositoryMock { get; }
        public Mock<IOrderRepository> OrderRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public CreateOrderUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            LoggedUserMock = new Mock<ILoggedUser>();
            ClientRepositoryMock = new Mock<IClientRepository>();
            AddressRepositoryMock = new Mock<IAddressRepository>();
            CartRepositoryMock = new Mock<ICartRepository>();
            OrderRepositoryMock = new Mock<IOrderRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public CreateOrderUseCase CreateUseCase()
        {
            return new CreateOrderUseCase(
                LoggedUserMock.Object,
                ClientRepositoryMock.Object,
                AddressRepositoryMock.Object,
                CartRepositoryMock.Object,
                OrderRepositoryMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public Client CreateValidClient()
        {
            return Client.NewClient(
                Faker.Name.FirstName(), Faker.Name.LastName(), Faker.Internet.Email(),
                "11999998888", "hashed_pass", null, null, false, Notification.Create()
            );
        }

        public Cart CreateCartWithItems(Guid clientId)
        {
            var cart = Cart.NewCart(clientId);
            cart.AddItem(Guid.NewGuid(), 2, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(50));
            cart.AddItem(Guid.NewGuid(), 1, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(100));
            return cart;
        }

        public Address CreateValidAddress(Guid clientId)
        {
            return Address.NewAddress(
                clientId, AddressType.Shipping, "01001000", "Praça da Sé", "1",
                "lado ímpar", "Sé", "São Paulo", "SP", true, Notification.Create()
            );
        }

        public CreateOrderInput GetValidInput(Guid shippingId, Guid billingId)
        {
            return new CreateOrderInput(shippingId, billingId, Faker.Random.Decimal(10, 25), "Entregar no período da manhã.");
        }
    }