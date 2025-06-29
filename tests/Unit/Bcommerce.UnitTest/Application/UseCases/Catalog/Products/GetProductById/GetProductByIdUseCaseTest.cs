using Bcommerce.Domain.Catalog.Products;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.GetProductById
{
    [Collection(nameof(GetProductByIdUseCaseTestFixture))]
    public class GetProductByIdUseCaseTest
    {
        private readonly GetProductByIdUseCaseTestFixture _fixture;

        public GetProductByIdUseCaseTest(GetProductByIdUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Obter Produto por ID com Sucesso")]
        [Trait("Application", "GetProductById - UseCase")]
        public async Task Execute_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.Get(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            // CORREÇÃO: O input do caso de uso é um Guid, não um objeto.
            var result = await useCase.Execute(product.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(product.Id);
            result.Value.Name.Should().Be(product.Name);

            _fixture.ProductRepositoryMock.Verify(r => r.Get(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Obter Produto Inexistente")]
        [Trait("Application", "GetProductById - UseCase")]
        public async Task Execute_WhenProductDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var nonExistentProductId = Guid.NewGuid();
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.Get(nonExistentProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await useCase.Execute(nonExistentProductId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error.Should().NotBeNull();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Produto não encontrado.");

            _fixture.ProductRepositoryMock.Verify(r => r.Get(nonExistentProductId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}