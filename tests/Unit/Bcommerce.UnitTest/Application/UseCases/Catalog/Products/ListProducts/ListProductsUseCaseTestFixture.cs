using Bcomerce.Application.UseCases.Catalog.Products.ListProducts;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.ListProducts
{
    [CollectionDefinition(nameof(ListProductsUseCaseTestFixture))]
    public class ListProductsUseCaseTestFixtureCollection : ICollectionFixture<ListProductsUseCaseTestFixture> { }

    public class ListProductsUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }

        public ListProductsUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ProductRepositoryMock = new Mock<IProductRepository>();
        }

        public ListProductsUseCase CreateUseCase()
        {
            return new ListProductsUseCase(ProductRepositoryMock.Object);
        }

        public ListProductsInput GetValidInput()
        {
            return new ListProductsInput(
                Page: 1,
                PageSize: 10,
                SearchTerm: null,
                CategoryId: null,
                BrandId: null,
                SortBy: "name",
                SortDirection: "asc"
            );
        }

        public List<Product> CreateValidProducts(int count)
        {
            // CORREÇÃO: Chamando o método de fábrica com todos os argumentos necessários.
            return Enumerable.Range(1, count).Select(_ => Product.NewProduct(
                Faker.Commerce.Ean13(),
                Faker.Commerce.ProductName(),
                Faker.Lorem.Sentence(),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 1000))),
                Faker.Random.Int(1, 100),
                System.Guid.NewGuid(),
                System.Guid.NewGuid(),
                Dimensions.Create(1, 1, 1, 1),
                Notification.Create()
            )).ToList();
        }
    }
}