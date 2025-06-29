using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Catalog.Brands.Events;
using Bcommerce.Domain.Validation.Handlers;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Brands
{
    [Collection(nameof(BrandTestFixture))]
    public class BrandUnitTest
    {
        private readonly BrandTestFixture _fixture;

        public BrandUnitTest(BrandTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Marca Válida e Disparar Evento")]
        [Trait("Domain", "Brand - Aggregate")]
        public void NewBrand_WithValidData_ShouldCreateSuccessfullyAndRaiseEvent()
        {
            // Arrange
            var handler = Notification.Create();
            var (name, description, logoUrl) = _fixture.GetValidBrandInputData();

            // Act
            var brand = Brand.NewBrand(name, description, logoUrl, handler);

            // Assert
            handler.HasError().Should().BeFalse();
            brand.Should().NotBeNull();
            brand.Name.Should().Be(name);
            brand.IsActive.Should().BeTrue();
            brand.Slug.Should().Be(name.ToLowerInvariant().Replace(" ", "-"));
            brand.Events.Should().HaveCount(1);
            brand.Events.First().Should().BeOfType<BrandCreatedEvent>();
        }

        [Theory(DisplayName = "Não Deve Criar Marca com Nome Inválido")]
        [Trait("Domain", "Brand - Aggregate")]
        [InlineData("", "'Name' da marca é obrigatório.")]
        [InlineData("   ", "'Name' da marca é obrigatório.")]
        [InlineData(null, "'Name' da marca é obrigatório.")]
        public void NewBrand_WithInvalidName_ShouldReturnError(string invalidName, string expectedErrorMessage)
        {
            // Arrange
            var handler = Notification.Create();

            // Act
            var brand = Brand.NewBrand(invalidName, "Descrição válida", null, handler);

            // Assert
            handler.HasError().Should().BeTrue();
            handler.FirstError()!.Message.Should().Be(expectedErrorMessage);
            brand.Events.Should().BeEmpty();
        }

        [Fact(DisplayName = "Deve Atualizar Marca com Sucesso e Disparar Evento")]
        [Trait("Domain", "Brand - Aggregate")]
        public void Update_WithValidData_ShouldUpdateBrandAndRaiseEvent()
        {
            // Arrange
            var brand = _fixture.CreateValidBrand();
            var newName = _fixture.Faker.Company.CompanyName();
            var newDescription = _fixture.Faker.Lorem.Sentence();
            var newLogoUrl = _fixture.Faker.Internet.Url();
            var handler = Notification.Create();

            // Act
            // CORREÇÃO: A chamada agora está correta, sem o parâmetro 'isActive'.
            brand.Update(newName, newDescription, newLogoUrl, handler);

            // Assert
            handler.HasError().Should().BeFalse();
            brand.Name.Should().Be(newName);
            brand.Description.Should().Be(newDescription);
            brand.LogoUrl.Should().Be(newLogoUrl);
            brand.Events.Should().HaveCount(1);
            brand.Events.First().Should().BeOfType<BrandUpdatedEvent>();
        }

        [Fact(DisplayName = "Deve Desativar uma Marca Ativa")]
        [Trait("Domain", "Brand - Aggregate")]
        public void Deactivate_WhenBrandIsActive_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var brand = _fixture.CreateValidBrand();
            brand.IsActive.Should().BeTrue(); // Garante que a marca começa ativa

            // Act
            brand.Deactivate();

            // Assert
            brand.IsActive.Should().BeFalse();
        }
    }
}