using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.AddItemToCart
{
    [CollectionDefinition(nameof(AddItemToCartUseCaseTestFixture))]
    public class AddItemToCartUseCaseTestFixtureCollection : ICollectionFixture<AddItemToCartUseCaseTestFixture> { }

    public class AddItemToCartUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }
        public Mock<ICartRepository> CartRepositoryMock { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public AddItemToCartUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            LoggedUserMock = new Mock<ILoggedUser>();
            CartRepositoryMock = new Mock<ICartRepository>();
            ProductRepositoryMock = new Mock<IProductRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public AddItemToCartUseCase CreateUseCase()
        {
            return new AddItemToCartUseCase(
                LoggedUserMock.Object,
                CartRepositoryMock.Object,
                ProductRepositoryMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public AddItemToCartInput GetValidInput()
        {
            return new AddItemToCartInput(
                ProductVariantId: Guid.NewGuid(),
                Quantity: Faker.Random.Int(1, 5)
            );
        }

        public Product CreateValidProduct()
        {
            // CORREÇÃO: Chamando o método de fábrica com todos os argumentos necessários.
            return Product.NewProduct(
                Faker.Commerce.Ean13(),
                Faker.Commerce.ProductName(),
                Faker.Lorem.Sentence(),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 1000))),
                Faker.Random.Int(1, 100),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Dimensions.Create(1, 1, 1, 1),
                Notification.Create()
            );
        }

        public Cart CreateValidCart(Guid clientId)
        {
            return Cart.NewCart(clientId);
        }
    }
}