using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Products;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.ListPublicProducts
{
    [Collection(nameof(ListPublicProductsUseCaseTestFixture))]
    public class ListPublicProductsUseCaseTest
    {
        private readonly ListPublicProductsUseCaseTestFixture _fixture;

        public ListPublicProductsUseCaseTest(ListPublicProductsUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.BrandRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Listar Produtos Públicos Paginados")]
        [Trait("Application", "ListPublicProducts - UseCase")]
        public async Task Execute_WhenCalled_ShouldReturnPaginatedList()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var products = _fixture.CreateValidProducts(5);
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock.Setup(r => r.ListAsync(input.Page, input.PageSize, input.SearchTerm, null, null, input.SortBy, input.SortDirection, It.IsAny<CancellationToken>())).ReturnsAsync(products);
            _fixture.ProductRepositoryMock.Setup(r => r.CountAsync(input.SearchTerm, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(15);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(5);
            result.Value.TotalCount.Should().Be(15);
            result.Value.Page.Should().Be(input.Page);
            result.Value.TotalPages.Should().Be(2); // 15 itens / 10 por página = 2 páginas
        }

        [Fact(DisplayName = "Deve Filtrar Produtos por Slug de Categoria")]
        [Trait("Application", "ListPublicProducts - UseCase")]
        public async Task Execute_WhenCategorySlugIsProvided_ShouldFilterByCategoryId()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var input = _fixture.GetValidInput() with { CategorySlug = category.Slug };
            var useCase = _fixture.CreateUseCase();

            // Configura o mock para encontrar a categoria pelo slug
            _fixture.CategoryRepositoryMock.Setup(r => r.GetBySlugAsync(category.Slug, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            
            // Act
            await useCase.Execute(input);

            // Assert
            // Verifica se o repositório de produto foi chamado com o ID da categoria correto
            _fixture.ProductRepositoryMock.Verify(r => r.ListAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), 
                category.Id, // <-- Ponto principal da verificação
                null, 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
            ), Times.Once);

            _fixture.ProductRepositoryMock.Verify(r => r.CountAsync(
                It.IsAny<string>(),
                category.Id, // <-- Ponto principal da verificação
                null,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }
    }
}