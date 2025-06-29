using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.ListCategories
{
    [Collection(nameof(ListCategoriesUseCaseTestFixture))]
    public class ListCategoriesUseCaseTest
    {
        private readonly ListCategoriesUseCaseTestFixture _fixture;

        public ListCategoriesUseCaseTest(ListCategoriesUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CategoryRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Listar Categorias com Sucesso")]
        [Trait("Application", "ListCategories - UseCase")]
        public async Task Execute_WhenCategoriesExist_ShouldReturnCategoryList()
        {
            // Arrange
            var categories = _fixture.CreateValidCategories(3);
            var useCase = _fixture.CreateUseCase();
            var input = _fixture.GetValidInput();

            // CORREÇÃO: O método correto no repositório é 'GetAllAsync'.
            _fixture.CategoryRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            // CORREÇÃO: Passando o objeto de input para o UseCase.
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(3);
            
            _fixture.CategoryRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Deve Retornar Lista Vazia se Não Houver Categorias")]
        [Trait("Application", "ListCategories - UseCase")]
        public async Task Execute_WhenNoCategoriesExist_ShouldReturnEmptyList()
        {
            // Arrange
            var useCase = _fixture.CreateUseCase();
            var input = _fixture.GetValidInput();

            // CORREÇÃO: Configurando o mock para o método correto.
            _fixture.CategoryRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bcommerce.Domain.Catalog.Categories.Category>());

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();

            _fixture.CategoryRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}