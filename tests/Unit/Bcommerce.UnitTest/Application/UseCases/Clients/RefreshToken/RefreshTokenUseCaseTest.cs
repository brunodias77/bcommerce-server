using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Services;
using FluentAssertions;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Catalog.Clients.RefreshToken;
using Xunit;
using System; // Adicionado para DateTime

namespace Bcommerce.UnitTest.Application.UseCases.Clients.RefreshToken
{
    [Collection(nameof(RefreshTokenUseCaseTestFixture))]
    public class RefreshTokenUseCaseTest
    {
        private readonly RefreshTokenUseCaseTestFixture _fixture;

        public RefreshTokenUseCaseTest(RefreshTokenUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ClientRepositoryMock.Invocations.Clear();
            _fixture.RefreshTokenRepositoryMock.Invocations.Clear();
            _fixture.TokenServiceMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Gerar Novos Tokens com Sucesso")]
        [Trait("Application", "RefreshToken - UseCase")]
        public async Task Execute_WhenRefreshTokenIsValid_ShouldRotateTokensAndCommit()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var oldRefreshToken = _fixture.CreateValidRefreshToken(client.Id);
            var input = new RefreshTokenInput(oldRefreshToken.TokenValue);
            var useCase = _fixture.CreateUseCase();
            var newAuthResult = new AuthResult("new_access_token", DateTime.UtcNow, "new_refresh_token");

            _fixture.RefreshTokenRepositoryMock.Setup(r => r.GetByTokenValueAsync(input.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(oldRefreshToken);
            _fixture.ClientRepositoryMock.Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.TokenServiceMock.Setup(s => s.GenerateTokens(client)).Returns(newAuthResult);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.AccessToken.Should().Be("new_access_token");

            // Verifica se o token antigo foi revogado e o novo foi salvo (rotação de token)
            _fixture.RefreshTokenRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Bcommerce.Domain.Customers.Clients.Entities.RefreshToken>(t => t.Id == oldRefreshToken.Id && t.RevokedAt != null), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.RefreshTokenRepositoryMock.Verify(r => r.AddAsync(It.Is<Bcommerce.Domain.Customers.Clients.Entities.RefreshToken>(t => t.TokenValue == newAuthResult.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Gerar Novos Tokens com Refresh Token Inválido ou Inativo")]
        [Trait("Application", "RefreshToken - UseCase")]
        public async Task Execute_WhenRefreshTokenIsInvalid_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            // Simula que o token não foi encontrado no repositório
            _fixture.RefreshTokenRepositoryMock.Setup(r => r.GetByTokenValueAsync(input.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync((Bcommerce.Domain.Customers.Clients.Entities.RefreshToken)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Sessão inválida. Por favor, realize o login novamente.");
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Rollback(), Times.Once);
        }
    }
}