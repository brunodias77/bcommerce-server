using Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;
using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.ListPublicProducts
{
    [CollectionDefinition(nameof(ListPublicProductsUseCaseTestFixture))]
    public class ListPublicProductsUseCaseTestFixtureCollection : ICollectionFixture<ListPublicProductsUseCaseTestFixture> { }

    public class ListPublicProductsUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }
        // CORREÇÃO: Mocks para todas as dependências
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }
        public Mock<IBrandRepository> BrandRepositoryMock { get; }

        public ListPublicProductsUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ProductRepositoryMock = new Mock<IProductRepository>();
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
            BrandRepositoryMock = new Mock<IBrandRepository>();
        }

        public ListPublicProductsUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando todas as dependências
            return new ListPublicProductsUseCase(
                ProductRepositoryMock.Object,
                CategoryRepositoryMock.Object,
                BrandRepositoryMock.Object
            );
        }

        public ListPublicProductsInput GetValidInput() => new();

        public List<Product> CreateValidProducts(int count)
        {
            return Enumerable.Range(1, count).Select(_ => Product.NewProduct(
                Faker.Commerce.Ean13(), Faker.Commerce.ProductName(), Faker.Lorem.Sentence(),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 1000))),
                Faker.Random.Int(1, 100), System.Guid.NewGuid(), System.Guid.NewGuid(),
                Dimensions.Create(1, 1, 1, 1), Notification.Create()
            )).ToList();
        }

        public Category CreateValidCategory()
        {
            return Category.NewCategory(
                Faker.Commerce.Categories(1)[0], Faker.Lorem.Sentence(), null, 
                Faker.Random.Int(0, 100), Notification.Create()
            );
        }
    }
}