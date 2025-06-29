using Bcomerce.Application.UseCases.Sales.Carts.RemoveCartItem;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.RemoveCartItem;

[CollectionDefinition(nameof(RemoveCartItemUseCaseTestFixture))]
public class RemoveCartItemUseCaseTestFixtureCollection : ICollectionFixture<RemoveCartItemUseCaseTestFixture> { }

public class RemoveCartItemUseCaseTestFixture
{
    public Mock<ILoggedUser> LoggedUserMock { get; }
    public Mock<ICartRepository> CartRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public RemoveCartItemUseCaseTestFixture()
    {
        LoggedUserMock = new Mock<ILoggedUser>();
        CartRepositoryMock = new Mock<ICartRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public RemoveCartItemUseCase CreateUseCase()
    {
        return new RemoveCartItemUseCase(
            LoggedUserMock.Object,
            CartRepositoryMock.Object,
            UnitOfWorkMock.Object
        );
    }

    public Cart CreateCartWithItems(Guid clientId, int itemCount = 2)
    {
        var cart = Cart.NewCart(clientId);
        for (int i = 0; i < itemCount; i++)
        {
            cart.AddItem(Guid.NewGuid(), 1, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(10));
        }
        return cart;
    }
}