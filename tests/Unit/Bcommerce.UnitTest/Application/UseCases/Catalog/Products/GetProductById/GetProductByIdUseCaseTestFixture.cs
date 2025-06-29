using Bcomerce.Application.UseCases.Catalog.Products.GetProductById;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.GetProductById
{
    [CollectionDefinition(nameof(GetProductByIdUseCaseTestFixture))]
    public class GetProductByIdUseCaseTestFixtureCollection : ICollectionFixture<GetProductByIdUseCaseTestFixture> { }

    public class GetProductByIdUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }

        public GetProductByIdUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ProductRepositoryMock = new Mock<IProductRepository>();
        }

        public GetProductByIdUseCase CreateUseCase()
        {
            return new GetProductByIdUseCase(ProductRepositoryMock.Object);
        }

        public Product CreateValidProduct()
        {
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
    }
}