using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Products;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.UpdateProduct
{
    [Collection(nameof(UpdateProductUseCaseTestFixture))]
    public class UpdateProductUseCaseTest
    {
        private readonly UpdateProductUseCaseTestFixture _fixture;

        public UpdateProductUseCaseTest(UpdateProductUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.BrandRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Atualizar Produto com Sucesso")]
        [Trait("Application", "UpdateProduct - UseCase")]
        public async Task Execute_WhenInputIsValid_ShouldUpdateProduct()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var category = _fixture.CreateValidCategory();
            var brand = _fixture.CreateValidBrand();
            var input = _fixture.GetValidInput(product.Id, category.Id, brand.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.Get(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Name.Should().Be(input.Name);
            _fixture.ProductRepositoryMock.Verify(r => r.Update(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Atualizar Produto se Não Encontrado")]
        [Trait("Application", "UpdateProduct - UseCase")]
        public async Task Execute_WhenProductNotFound_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidInput(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.Get(input.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Produto não encontrado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Atualizar Produto se Categoria Não For Encontrada")]
        [Trait("Application", "UpdateProduct - UseCase")]
        public async Task Execute_WhenCategoryNotFound_ShouldReturnError()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var input = _fixture.GetValidInput(product.Id, Guid.NewGuid(), Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.Get(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message.Contains("não foi encontrada"));
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Deve Fazer Rollback em Caso de Erro de Banco de Dados")]
        [Trait("Application", "UpdateProduct - UseCase")]
        public async Task Execute_WhenDatabaseThrows_ShouldReturnErrorAndRollback()
        {
            // Arrange
            var product = _fixture.CreateValidProduct();
            var category = _fixture.CreateValidCategory();
            var brand = _fixture.CreateValidBrand();
            var input = _fixture.GetValidInput(product.Id, category.Id, brand.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.Get(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(brand);
            _fixture.ProductRepositoryMock.Setup(r => r.Update(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Ocorreu um erro no banco de dados ao atualizar o produto.");
            _fixture.UnitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}