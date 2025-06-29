using Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;

[CollectionDefinition(nameof(UpdateCartItemQuantityUseCaseTestFixture))]
public class UpdateCartItemQuantityUseCaseTestFixtureCollection : ICollectionFixture<UpdateCartItemQuantityUseCaseTestFixture> { }

public class UpdateCartItemQuantityUseCaseTestFixture
{
    public Faker Faker { get; }
    public Mock<ILoggedUser> LoggedUserMock { get; }
    public Mock<ICartRepository> CartRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public UpdateCartItemQuantityUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        LoggedUserMock = new Mock<ILoggedUser>();
        CartRepositoryMock = new Mock<ICartRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public UpdateCartItemQuantityUseCase CreateUseCase()
    {
        return new UpdateCartItemQuantityUseCase(
            LoggedUserMock.Object,
            CartRepositoryMock.Object,
            UnitOfWorkMock.Object
        );
    }

    public Cart CreateCartWithItems(Guid clientId, int itemCount = 1)
    {
        var cart = Cart.NewCart(clientId);
        for (int i = 0; i < itemCount; i++)
        {
            cart.AddItem(Guid.NewGuid(), 2, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(10m * (i + 1)));
        }
        return cart;
    }

    public UpdateCartItemQuantityInput GetValidInput(Guid cartItemId, int newQuantity)
    {
        return new UpdateCartItemQuantityInput(cartItemId, newQuantity);
    }
}