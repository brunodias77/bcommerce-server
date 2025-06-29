using Bcomerce.Application.UseCases.Sales.Carts.GetCart;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.GetCart;

[CollectionDefinition(nameof(GetCartUseCaseTestFixture))]
public class GetCartUseCaseTestFixtureCollection : ICollectionFixture<GetCartUseCaseTestFixture> { }

public class GetCartUseCaseTestFixture
{
    public Faker Faker { get; }
    public Mock<ILoggedUser> LoggedUserMock { get; }
    public Mock<ICartRepository> CartRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public GetCartUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        LoggedUserMock = new Mock<ILoggedUser>();
        CartRepositoryMock = new Mock<ICartRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public GetCartUseCase CreateUseCase()
    {
        return new GetCartUseCase(
            LoggedUserMock.Object,
            CartRepositoryMock.Object,
            UnitOfWorkMock.Object
        );
    }

    public Cart CreateValidCart(Guid clientId)
    {
        var cart = Cart.NewCart(clientId);
        // Adiciona um item para simular um carrinho existente com conteúdo
        cart.AddItem(Guid.NewGuid(), 2, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(50));
        return cart;
    }
}