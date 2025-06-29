using Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.GetPublicProduct
{
    [CollectionDefinition(nameof(GetPublicProductBySlugUseCaseTestFixture))]
    public class GetPublicProductBySlugUseCaseTestFixtureCollection : ICollectionFixture<GetPublicProductBySlugUseCaseTestFixture> { }

    public class GetPublicProductBySlugUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IProductRepository> ProductRepositoryMock { get; }

        public GetPublicProductBySlugUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ProductRepositoryMock = new Mock<IProductRepository>();
        }

        public GetPublicProductBySlugUseCase CreateUseCase()
        {
            return new GetPublicProductBySlugUseCase(ProductRepositoryMock.Object);
        }

        public Product CreateValidProduct(bool isActive = true)
        {
            var product = Product.NewProduct(
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

            // A entidade Product não tem um método público para alterar 'IsActive'
            // diretamente após a criação, o que é uma boa prática de DDD.
            // Para o teste, podemos usar o método 'With' para reconstruir a entidade
            // no estado desejado (ativo ou inativo).
            if (!isActive)
            {
                return Product.With(
                    product.Id, product.BaseSku, product.Name, product.Slug, product.Description,
                    product.BasePrice, product.SalePrice, product.SalePriceStartDate, product.SalePriceEndDate,
                    product.StockQuantity, false, product.Dimensions, product.CategoryId, product.BrandId,
                    product.CreatedAt, product.UpdatedAt, null, null
                );
            }

            return product;
        }
    }
}