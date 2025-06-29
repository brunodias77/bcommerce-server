using Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Marketing.Coupons;
using Bcommerce.Domain.Marketing.Coupons.Repositories;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Marketing.Coupons.ApplyCoupon;

[CollectionDefinition(nameof(ApplyCouponUseCaseTestFixture))]
    public class ApplyCouponUseCaseTestFixtureCollection : ICollectionFixture<ApplyCouponUseCaseTestFixture> { }

    public class ApplyCouponUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }
        public Mock<IOrderRepository> OrderRepositoryMock { get; }
        public Mock<ICouponRepository> CouponRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public ApplyCouponUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            LoggedUserMock = new Mock<ILoggedUser>();
            OrderRepositoryMock = new Mock<IOrderRepository>();
            CouponRepositoryMock = new Mock<ICouponRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public ApplyCouponUseCase CreateUseCase()
        {
            return new ApplyCouponUseCase(
                LoggedUserMock.Object,
                OrderRepositoryMock.Object,
                CouponRepositoryMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public Order CreateValidPendingOrder(Guid clientId)
        {
            // O método de criação de pedido a partir do carrinho já o deixa no estado 'Pending'.
            var client = Client.NewClient("Test", "User", "test@test.com", "123", "hash", null, null, false, Notification.Create());
            typeof(Client).GetProperty("Id").SetValue(client, clientId); // Força o ID do cliente

            var cartItems = new List<CartItem>
            {
                CartItem.NewItem(Guid.NewGuid(), Guid.NewGuid(), 2, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(100))
            };
            var address = Bcommerce.Domain.Customers.Clients.Entities.Address.NewAddress(clientId, Bcommerce.Domain.Customers.Clients.Enums.AddressType.Shipping, "12345678", "street", "123", null, "neigh", "city", "ST", false, Notification.Create());

            return Order.NewOrderFromCart(client, cartItems, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(20), address, address);
        }

        public Coupon CreateValidCoupon()
        {
            return Coupon.NewPercentageCoupon("VALIDO10", 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10), Notification.Create());
        }

        public ApplyCouponInput GetValidInput(Guid orderId, string couponCode)
        {
            return new ApplyCouponInput(orderId, couponCode);
        }
    }