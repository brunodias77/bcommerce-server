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

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.CreateProduct
{
    [Collection(nameof(CreateProductUseCaseTestFixture))]
    public class CreateProductUseCaseTest
    {
        private readonly CreateProductUseCaseTestFixture _fixture;

        public CreateProductUseCaseTest(CreateProductUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.BrandRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Criar Produto com Sucesso")]
        [Trait("Application", "CreateProduct - UseCase")]
        public async Task Execute_WhenInputIsValid_ShouldCreateProduct()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var brand = _fixture.CreateValidBrand();
            var input = _fixture.GetValidInput(category.Id, brand.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.GetByBaseSkuAsync(input.BaseSku, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(brand);
            _fixture.UnitOfWorkMock.Setup(uow => uow.Commit()).Returns(Task.CompletedTask);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.BaseSku.Should().Be(input.BaseSku);
            _fixture.ProductRepositoryMock.Verify(r => r.Insert(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Criar Produto se SKU Já Existir")]
        [Trait("Application", "CreateProduct - UseCase")]
        public async Task Execute_WhenSkuAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var brand = _fixture.CreateValidBrand();
            var input = _fixture.GetValidInput(category.Id, brand.Id);
            var useCase = _fixture.CreateUseCase();
            // CORREÇÃO: Usando a fixture para criar um produto válido em vez de Mock.Of<>()
            var existingProduct = _fixture.CreateValidProduct();

            _fixture.ProductRepositoryMock.Setup(r => r.GetByBaseSkuAsync(input.BaseSku, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(brand);
            
            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == $"Um produto com o SKU '{input.BaseSku}' já existe.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Criar Produto se Categoria Não For Encontrada")]
        [Trait("Application", "CreateProduct - UseCase")]
        public async Task Execute_WhenCategoryNotFound_ShouldReturnError()
        {
            var input = _fixture.GetValidInput(Guid.NewGuid(), Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.GetByBaseSkuAsync(input.BaseSku, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            var result = await useCase.Execute(input);

            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == $"A categoria com o ID '{input.CategoryId}' não foi encontrada.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
        
        [Fact(DisplayName = "Não Deve Criar Produto se Marca Não For Encontrada")]
        [Trait("Application", "CreateProduct - UseCase")]
        public async Task Execute_WhenBrandNotFound_ShouldReturnError()
        {
            var category = _fixture.CreateValidCategory();
            var input = _fixture.GetValidInput(category.Id, Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.GetByBaseSkuAsync(input.BaseSku, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((Brand)null);
            
            var result = await useCase.Execute(input);

            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == $"A marca com o ID '{input.BrandId!.Value}' não foi encontrada.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Theory(DisplayName = "Não Deve Criar Produto com Input Inválido")]
        [Trait("Application", "CreateProduct - UseCase")]
        [InlineData("", "Nome Válido", "'BaseSku' do produto é obrigatório.")]
        [InlineData("SKU-VALIDO", "", "'Name' do produto é obrigatório.")]
        public async Task Execute_WhenDomainValidationFails_ShouldReturnError(string sku, string name, string expectedErrorMessage)
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var brand = _fixture.CreateValidBrand();
            var input = _fixture.GetValidInput(category.Id, brand.Id) with { BaseSku = sku, Name = name };
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.GetByBaseSkuAsync(input.BaseSku, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);
            _fixture.CategoryRepositoryMock.Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _fixture.BrandRepositoryMock.Setup(r => r.Get(input.BrandId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(brand);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message.Contains(expectedErrorMessage));
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}