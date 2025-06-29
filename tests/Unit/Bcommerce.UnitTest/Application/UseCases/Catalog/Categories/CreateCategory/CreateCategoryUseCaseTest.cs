using Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;
using Bcommerce.Domain.Catalog.Categories;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Categories
{
    [Collection(nameof(CreateCategoryUseCaseTestFixture))]
    public class CreateCategoryUseCaseTest
    {
        private readonly CreateCategoryUseCaseTestFixture _fixture;

        public CreateCategoryUseCaseTest(CreateCategoryUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            // Limpa o histórico de chamadas dos mocks antes de cada teste
            _fixture.CategoryRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Criar Categoria com Sucesso")]
        [Trait("Application", "CreateCategory - UseCase")]
        public async Task Execute_WhenInputIsValid_ShouldCreateCategoryAndCommit()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            // Configura o mock para simular que a categoria não existe
            _fixture.CategoryRepositoryMock
                .Setup(repo => repo.ExistsWithNameAsync(input.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(input.Name);
            result.Error.Should().BeNull();

            // Verifica se os métodos de persistência foram chamados corretamente
            _fixture.CategoryRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Criar Categoria se Nome Já Existir")]
        [Trait("Application", "CreateCategory - UseCase")]
        public async Task Execute_WhenCategoryNameAlreadyExists_ShouldReturnErrorAndNotCommit()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            // Configura o mock para simular que a categoria JÁ existe
            _fixture.CategoryRepositoryMock
                .Setup(repo => repo.ExistsWithNameAsync(input.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error.Should().NotBeNull();
            result.Error.FirstError()!.Message.Should().Be($"Uma categoria com o nome '{input.Name}' já existe.");

            // Verifica que NENHUM método de persistência foi chamado
            _fixture.CategoryRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        }
    }
}