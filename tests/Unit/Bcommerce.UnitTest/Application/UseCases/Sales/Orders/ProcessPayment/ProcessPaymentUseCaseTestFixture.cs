
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Sales.Orders.ProcessPayment;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Orders.ProcessPayment
{
    [CollectionDefinition(nameof(ProcessPaymentUseCaseTestFixture))]
    public class ProcessPaymentUseCaseTestFixtureCollection : ICollectionFixture<ProcessPaymentUseCaseTestFixture> { }

    public class ProcessPaymentUseCaseTestFixture
    {
        public Mock<ILoggedUser> LoggedUserMock { get; }
        public Mock<IOrderRepository> OrderRepositoryMock { get; }
        public Mock<IPaymentGateway> PaymentGatewayMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public ProcessPaymentUseCaseTestFixture()
        {
            LoggedUserMock = new Mock<ILoggedUser>();
            OrderRepositoryMock = new Mock<IOrderRepository>();
            PaymentGatewayMock = new Mock<IPaymentGateway>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public ProcessPaymentUseCase CreateUseCase()
        {
            return new ProcessPaymentUseCase(
                LoggedUserMock.Object,
                OrderRepositoryMock.Object,
                PaymentGatewayMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public Order CreateValidPendingOrder(Guid clientId)
        {
            var client = Client.NewClient("Test", "User", "test@test.com", "123", "hash", null, null, false, Notification.Create());
            typeof(Client).GetProperty("Id").SetValue(client, clientId);

            var cartItems = new List<CartItem>
            {
                CartItem.NewItem(Guid.NewGuid(), Guid.NewGuid(), 1, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(200))
            };
            var address = Bcommerce.Domain.Customers.Clients.Entities.Address.NewAddress(clientId, Bcommerce.Domain.Customers.Clients.Enums.AddressType.Shipping, "12345678", "street", "123", null, "neigh", "city", "ST", false, Notification.Create());

            return Order.NewOrderFromCart(client, cartItems, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(15), address, address);
        }

        public ProcessPaymentInput GetValidInput(Guid orderId, string paymentToken)
        {
            return new ProcessPaymentInput(orderId, paymentToken);
        }
    }
}