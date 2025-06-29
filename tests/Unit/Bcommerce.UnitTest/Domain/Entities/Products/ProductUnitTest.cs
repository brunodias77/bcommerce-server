using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation.Handlers;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Products
{
    [Collection(nameof(ProductTestFixture))]
    public class ProductUnitTest
    {
        private readonly ProductTestFixture _fixture;

        public ProductUnitTest(ProductTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Produto com Dados Válidos")]
        [Trait("Domain", "Product - Aggregate")]
        public void NewProduct_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var handler = Notification.Create();
            var (baseSku, name, description, basePrice, stockQuantity, categoryId, brandId, dimensions) = _fixture.GetValidProductInput();

            // Act
            var product = Product.NewProduct(baseSku, name, description, basePrice, stockQuantity, categoryId, brandId, dimensions, handler);

            // Assert
            handler.HasError().Should().BeFalse();
            product.Should().NotBeNull();
            product.BaseSku.Should().Be(baseSku);
            product.Name.Should().Be(name);
            product.IsActive.Should().BeTrue();
            product.StockQuantity.Should().Be(stockQuantity);
            product.BasePrice.Should().Be(basePrice);
            product.Dimensions.Should().Be(dimensions);
            product.Events.Should().BeEmpty();
        }

        [Theory(DisplayName = "Não Deve Criar Produto com Nome ou SKU Inválido")]
        [Trait("Domain", "Product - Aggregate")]
        [InlineData("", "Nome Válido", "'BaseSku' do produto é obrigatório.")]
        [InlineData("SKU-VALIDO", "", "'Name' do produto é obrigatório.")]
        public void NewProduct_WithInvalidSkuOrName_ShouldAppendError(string sku, string name, string expectedError)
        {
            // Arrange
            var handler = Notification.Create();
            var (_, _, description, basePrice, stockQuantity, categoryId, brandId, dimensions) = _fixture.GetValidProductInput();

            // Act
            Product.NewProduct(sku, name, description, basePrice, stockQuantity, categoryId, brandId, dimensions, handler);

            // Assert
            handler.HasError().Should().BeTrue();
            handler.FirstError()!.Message.Should().Be(expectedError);
        }

        [Fact(DisplayName = "Não Deve Criar Produto com Preço Base Inválido")]
        [Trait("Domain", "Product - Aggregate")]
        public void NewProduct_WithInvalidBasePrice_ShouldAppendError()
        {
            // Arrange
            var handler = Notification.Create();
            var (baseSku, name, description, _, stockQuantity, categoryId, brandId, dimensions) = _fixture.GetValidProductInput();
            var invalidPrice = Money.Create(0); // Preço inválido conforme a regra de negócio

            // Act
            Product.NewProduct(baseSku, name, description, invalidPrice, stockQuantity, categoryId, brandId, dimensions, handler);

            // Assert
            handler.HasError().Should().BeTrue();
            handler.FirstError()!.Message.Should().Be("Produto deve ter um preço base positivo.");
        }

        [Fact(DisplayName = "Deve Adicionar Imagem Corretamente e Definir a Primeira como Capa")]
        [Trait("Domain", "Product - Aggregate")]
        public void AddImage_WhenCalledMultipleTimes_ShouldAddImagesAndSetFirstAsCover()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var imageUrl1 = _fixture.Faker.Image.PicsumUrl();
            var imageUrl2 = _fixture.Faker.Image.PicsumUrl();

            // Act
            product.AddImage(imageUrl1, "Imagem 1");
            product.AddImage(imageUrl2, "Imagem 2");

            // Assert
            product.Images.Should().HaveCount(2);
            var image1 = product.Images.First();
            var image2 = product.Images.Last();

            image1.ImageUrl.Should().Be(imageUrl1);
            image1.IsCover.Should().BeTrue();
            image1.SortOrder.Should().Be(0);

            image2.ImageUrl.Should().Be(imageUrl2);
            image2.IsCover.Should().BeFalse();
            image2.SortOrder.Should().Be(1);
        }

        [Fact(DisplayName = "Não Deve Definir Preço Promocional Maior ou Igual ao Base")]
        [Trait("Domain", "Product - Aggregate")]
        public void SetSalePrice_WhenPriceIsHigherThanBase_ShouldThrowException()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var invalidSalePrice = Money.Create(product.BasePrice.Amount + 10);
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(10);

            // Act
            Action action = () => product.SetSalePrice(invalidSalePrice, startDate, endDate);

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("Preço de oferta deve ser menor que o preço base.");
        }

        [Fact(DisplayName = "Não Deve Ajustar Estoque com Quantidade Negativa")]
        [Trait("Domain", "Product - Aggregate")]
        public void AdjustStock_WithNegativeQuantity_ShouldThrowException()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();

            // Act
            Action action = () => product.AdjustStock(-10);

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("A quantidade em estoque não pode ser negativa.");
        }
    }
}