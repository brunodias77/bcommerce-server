using Bcommerce.Domain.Catalog.Products;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Products.ListProducts
{
    [Collection(nameof(ListProductsUseCaseTestFixture))]
    public class ListProductsUseCaseTest
    {
        private readonly ListProductsUseCaseTestFixture _fixture;

        public ListProductsUseCaseTest(ListProductsUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ProductRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Listar Produtos Paginados com Sucesso")]
        [Trait("Application", "ListProducts - UseCase")]
        public async Task Execute_WhenProductsExist_ShouldReturnPaginatedList()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var products = _fixture.CreateValidProducts(5);
            var useCase = _fixture.CreateUseCase();

            // O `ListProductsUseCase` foi corrigido para retornar PagedListOutput,
            // mas o mock do repositório deve retornar a lista simples (IEnumerable).
            _fixture.ProductRepositoryMock
                .Setup(r => r.ListAsync(
                    input.Page, input.PageSize, input.SearchTerm, input.CategoryId, input.BrandId, input.SortBy, input.SortDirection, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);
            
            // O UseCase internamente deve chamar CountAsync para montar a paginação.
            // Aqui, o mock retorna um total de 15, mesmo que a página atual tenha 5.
            _fixture.ProductRepositoryMock
                .Setup(r => r.CountAsync(
                    input.SearchTerm, input.CategoryId, input.BrandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(15);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(5);
            result.Value.TotalCount.Should().Be(15);
            result.Value.Page.Should().Be(input.Page);
            result.Value.PageSize.Should().Be(input.PageSize);
        }

        [Fact(DisplayName = "Deve Retornar Lista Vazia e Paginada se Não Houver Produtos")]
        [Trait("Application", "ListProducts - UseCase")]
        public async Task Execute_WhenNoProductsExist_ShouldReturnEmptyPaginatedList()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            _fixture.ProductRepositoryMock
                .Setup(r => r.ListAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<System.Guid?>(), It.IsAny<System.Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());
            
            _fixture.ProductRepositoryMock
                .Setup(r => r.CountAsync(
                    It.IsAny<string>(), It.IsAny<System.Guid?>(), It.IsAny<System.Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            result.Value.TotalPages.Should().Be(0); // Nenhum item, nenhuma página
        }
    }
}