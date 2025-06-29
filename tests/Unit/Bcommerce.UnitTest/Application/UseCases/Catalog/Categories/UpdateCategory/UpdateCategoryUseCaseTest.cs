using Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;
using Bcommerce.Domain.Catalog.Categories;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.UpdateCategory
{
    [Collection(nameof(UpdateCategoryUseCaseTestFixture))]
    public class UpdateCategoryUseCaseTest
    {
        private readonly UpdateCategoryUseCaseTestFixture _fixture;

        public UpdateCategoryUseCaseTest(UpdateCategoryUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Atualizar Categoria com Sucesso")]
        [Trait("Application", "UpdateCategory - UseCase")]
        public async Task Execute_WhenInputIsValid_ShouldUpdateCategory()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var input = _fixture.GetValidInput(category.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            
            // CORREÇÃO: Usando o método correto 'ExistsWithNameAsync'
            _fixture.CategoryRepositoryMock
                .Setup(r => r.ExistsWithNameAsync(input.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Name.Should().Be(input.Name);
            result.Value.Description.Should().Be(input.Description);

            _fixture.CategoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Atualizar Categoria se Não Encontrada")]
        [Trait("Application", "UpdateCategory - UseCase")]
        public async Task Execute_WhenCategoryNotFound_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidInput(Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(input.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Categoria não encontrada.");
            _fixture.CategoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Atualizar se Nome Já Existir para Outra Categoria")]
        [Trait("Application", "UpdateCategory - UseCase")]
        public async Task Execute_WhenNameAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var categoryToUpdate = _fixture.CreateValidCategory();
            var input = _fixture.GetValidInput(categoryToUpdate.Id); // Input com novo nome
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(categoryToUpdate.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoryToUpdate);

            // CORREÇÃO: Mock para 'ExistsWithNameAsync' retornando true
            _fixture.CategoryRepositoryMock
                .Setup(r => r.ExistsWithNameAsync(input.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == $"Uma categoria com o nome '{input.Name}' já existe.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
        
        [Fact(DisplayName = "Não Deve Atualizar com Nome Inválido")]
        [Trait("Application", "UpdateCategory - UseCase")]
        public async Task Execute_WhenInputIsInvalid_ShouldReturnError()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            // Input inválido com nome vazio
            var input = _fixture.GetValidInput(category.Id) with { Name = "" };
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "'Name' não pode ser nulo ou vazio.");
            
            // CORREÇÃO: Commit não deve ser chamado se a validação falhar.
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}