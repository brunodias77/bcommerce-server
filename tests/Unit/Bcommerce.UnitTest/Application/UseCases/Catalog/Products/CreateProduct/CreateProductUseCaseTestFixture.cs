using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.CreateProduct
{
    [CollectionDefinition(nameof(CreateProductUseCaseTestFixture))]
    public class CreateProductUseCaseTestFixtureCollection : ICollectionFixture<CreateProductUseCaseTestFixture> { }

    public class CreateProductUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }
        public Mock<IBrandRepository> BrandRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public CreateProductUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ProductRepositoryMock = new Mock<IProductRepository>();
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
            BrandRepositoryMock = new Mock<IBrandRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public CreateProductUseCase CreateUseCase()
        {
            return new CreateProductUseCase(
                ProductRepositoryMock.Object,
                CategoryRepositoryMock.Object,
                BrandRepositoryMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public CreateProductInput GetValidInput(Guid categoryId, Guid? brandId)
        {
            return new CreateProductInput(
                BaseSku: Faker.Commerce.Ean13(),
                Name: Faker.Commerce.ProductName(),
                Description: Faker.Lorem.Sentence(),
                BasePrice: decimal.Parse(Faker.Commerce.Price(10, 1000)),
                StockQuantity: Faker.Random.Int(1, 100),
                CategoryId: categoryId,
                BrandId: brandId,
                WeightKg: Faker.Random.Decimal(0.1m, 5),
                HeightCm: Faker.Random.Int(10, 100),
                WidthCm: Faker.Random.Int(10, 100),
                DepthCm: Faker.Random.Int(10, 100)
            );
        }

        public Product CreateValidProduct()
        {
            return Product.NewProduct(
                Faker.Commerce.Ean13(), Faker.Commerce.ProductName(), Faker.Lorem.Sentence(),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 1000))),
                Faker.Random.Int(1, 100), Guid.NewGuid(), Guid.NewGuid(),
                Dimensions.Create(1, 1, 1, 1), Notification.Create()
            );
        }

        public Category CreateValidCategory()
        {
            // CORREÇÃO: Adicionando o parâmetro 'sortOrder' que faltava.
            return Category.NewCategory(
                Faker.Commerce.Categories(1)[0], Faker.Lorem.Sentence(), null,
                Faker.Random.Int(0, 100), // sortOrder
                Notification.Create()
            );
        }

        public Brand CreateValidBrand()
        {
            return Brand.NewBrand(
                Faker.Company.CompanyName(), Faker.Lorem.Sentence(), null,
                Notification.Create()
            );
        }
    }
}