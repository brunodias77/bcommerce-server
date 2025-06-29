using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Products
{
    /// <summary>
    /// Fixture para os testes da entidade Product.
    /// Centraliza a geração de dados e a criação de instâncias válidas.
    /// </summary>
    public class ProductTestFixture
    {
        public Faker Faker { get; }

        public ProductTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        /// <summary>
        /// Gera um conjunto de dados de entrada válidos para criar um produto.
        /// </summary>
        public (string BaseSku, string Name, string? Description, Money BasePrice, int StockQuantity, Guid CategoryId, Guid? BrandId, Dimensions Dimensions) GetValidProductInput()
        {
            return (
                Faker.Commerce.Ean13(),
                Faker.Commerce.ProductName(),
                Faker.Commerce.ProductDescription(),
                Money.Create(decimal.Parse(Faker.Commerce.Price(10, 1000))),
                Faker.Random.Number(0, 100), // Estoque pode ser zero
                Guid.NewGuid(),
                Guid.NewGuid(),
                Dimensions.Create(
                    Faker.Random.Decimal(0.1m, 5),
                    Faker.Random.Int(10, 100),
                    Faker.Random.Int(10, 100),
                    Faker.Random.Int(10, 100)
                )
            );
        }

        /// <summary>
        /// Cria uma instância válida da entidade Product, pronta para ser usada nos testes.
        /// </summary>
        public Product CreateValidProduct()
        {
            var (baseSku, name, description, basePrice, stockQuantity, categoryId, brandId, dimensions) = GetValidProductInput();
            var product = Product.NewProduct(
                baseSku,
                name,
                description,
                basePrice,
                stockQuantity,
                categoryId,
                brandId,
                dimensions,
                Notification.Create()
            );
            // Limpa eventos da criação para focar nos eventos do teste atual
            product.ClearEvents();
            return product;
        }
    }

    /// <summary>
    /// Define a coleção para a fixture, garantindo uma única instância para os testes.
    /// </summary>
    [CollectionDefinition(nameof(ProductTestFixture))]
    public class ProductTestFixtureCollection : ICollectionFixture<ProductTestFixture> { }
}