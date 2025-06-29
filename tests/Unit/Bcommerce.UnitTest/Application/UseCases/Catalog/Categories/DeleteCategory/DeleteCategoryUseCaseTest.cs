using Bcommerce.Domain.Catalog.Categories;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.DeleteCategory
{
    [Collection(nameof(DeleteCategoryUseCaseTestFixture))]
    public class DeleteCategoryUseCaseTest
    {
        private readonly DeleteCategoryUseCaseTestFixture _fixture;

        public DeleteCategoryUseCaseTest(DeleteCategoryUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Excluir Categoria com Sucesso")]
        [Trait("Application", "DeleteCategory - UseCase")]
        public async Task Execute_WhenCategoryExists_ShouldDeleteAndCommit()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var useCase = _fixture.CreateUseCase();

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // ADICIONE ESTAS DUAS CONFIGURAÇÕES DE MOCK
            _fixture.CategoryRepositoryMock
                .Setup(r => r.Delete(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            _fixture.UnitOfWorkMock
                .Setup(uow => uow.Commit())
                .Returns(Task.CompletedTask);

            // Act
            var result = await useCase.Execute(category.Id);

            // Assert
            result.IsSuccess.Should().BeTrue(); // Agora vai passar!
            result.Value.Should().BeTrue();
            result.Error.Should().BeNull();
            _fixture.CategoryRepositoryMock.Verify(r => r.Delete(category, It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Excluir Categoria Inexistente")]
        [Trait("Application", "DeleteCategory - UseCase")]
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
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Categoria não encontrada.");
            _fixture.CategoryRepositoryMock.Verify(r => r.Delete(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Deve Fazer Rollback em Caso de Exceção no Repositório")]
        [Trait("Application", "DeleteCategory - UseCase")]
        public async Task Execute_WhenRepositoryThrowsException_ShouldReturnErrorAndRollback()
        {
            // Arrange
            var category = _fixture.CreateValidCategory();
            var useCase = _fixture.CreateUseCase();
            var dbException = new Exception("Erro de banco de dados");

            _fixture.CategoryRepositoryMock
                .Setup(r => r.Get(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            
            _fixture.CategoryRepositoryMock
                .Setup(r => r.Delete(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbException);

            // Act
            var result = await useCase.Execute(category.Id);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Ocorreu um erro no banco de dados ao excluir a categoria.");
            _fixture.UnitOfWorkMock.Verify(uow => uow.Rollback(), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        }
    }
}