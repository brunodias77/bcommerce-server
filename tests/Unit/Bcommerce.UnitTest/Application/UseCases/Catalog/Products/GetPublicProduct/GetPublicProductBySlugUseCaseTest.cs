using Bcommerce.Domain.Catalog.Products;
using FluentAssertions;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.GetPublicProduct
{
    [Collection(nameof(GetPublicProductBySlugUseCaseTestFixture))]
    public class GetPublicProductBySlugUseCaseTest
    {
        private readonly GetPublicProductBySlugUseCaseTestFixture _fixture;

        public GetPublicProductBySlugUseCaseTest(GetPublicProductBySlugUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Obter Produto Público por Slug com Sucesso")]
        [Trait("Application", "GetPublicProductBySlug - UseCase")]
        public async Task Execute_WhenProductExistsAndIsActive_ShouldReturnProduct()
        {
            // Arrange
            var product = _fixture.CreateValidProduct(isActive: true);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.GetBySlugAsync(product.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            // CORREÇÃO: O input do caso de uso é uma string, não um objeto.
            var result = await useCase.Execute(product.Slug);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Slug.Should().Be(product.Slug);

            _fixture.ProductRepositoryMock.Verify(r => r.GetBySlugAsync(product.Slug, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Obter Produto Inexistente")]
        [Trait("Application", "GetPublicProductBySlug - UseCase")]
        public async Task Execute_WhenProductDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var nonExistentSlug = "slug-que-nao-existe";
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.GetBySlugAsync(nonExistentSlug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await useCase.Execute(nonExistentSlug);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Produto não encontrado.");
            
            _fixture.ProductRepositoryMock.Verify(r => r.GetBySlugAsync(nonExistentSlug, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Obter Produto se Estiver Inativo")]
        [Trait("Application", "GetPublicProductBySlug - UseCase")]
        public async Task Execute_WhenProductIsInactive_ShouldReturnError()
        {
            // Arrange
            var inactiveProduct = _fixture.CreateValidProduct(isActive: false);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.GetBySlugAsync(inactiveProduct.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveProduct);

            // Act
            var result = await useCase.Execute(inactiveProduct.Slug);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Produto não encontrado.");

            _fixture.ProductRepositoryMock.Verify(r => r.GetBySlugAsync(inactiveProduct.Slug, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}