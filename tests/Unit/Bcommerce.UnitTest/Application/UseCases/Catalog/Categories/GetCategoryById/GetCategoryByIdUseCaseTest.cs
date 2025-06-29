using Bcommerce.Domain.Catalog.Categories;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.GetCategoryById
{
    [Collection(nameof(GetCategoryByIdUseCaseTestFixture))]
    public class GetCategoryByIdUseCaseTest
    {
        private readonly GetCategoryByIdUseCaseTestFixture _fixture;

        public GetCategoryByIdUseCaseTest(GetCategoryByIdUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CategoryRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Obter Categoria por ID com Sucesso")]
        [Trait("Application", "GetCategoryById - UseCase")]
        public async Task Execute_WhenCategoryExists_ShouldReturnCategory()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            // CORREÇÃO: O input do caso de uso é um Guid, não um objeto.
            var result = await useCase.Execute(category.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(category.Id);
            result.Value.Name.Should().Be(category.Name);

            _fixture.CategoryRepositoryMock.Verify(r => r.Get(category.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Obter Categoria Inexistente")]
        [Trait("Application", "GetCategoryById - UseCase")]
        public async Task Execute_WhenCategoryDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var nonExistentCategoryId = Guid.NewGuid();
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(nonExistentCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await useCase.Execute(nonExistentCategoryId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error.Should().NotBeNull();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Categoria não encontrada.");

            _fixture.CategoryRepositoryMock.Verify(r => r.Get(nonExistentCategoryId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}