using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Logout
{
    [Collection(nameof(LogoutUseCaseTestFixture))]
    public class LogoutUseCaseTest
    {
        private readonly LogoutUseCaseTestFixture _fixture;

        public LogoutUseCaseTest(LogoutUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.RevokedTokenRepositoryMock.Invocations.Clear();
            _fixture.HttpContextAccessorMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Fazer Logout com Sucesso para Usuário Autenticado")]
        [Trait("Application", "Logout - UseCase")]
        public async Task Execute_WhenUserIsAuthenticated_ShouldAddTokenToDenyList()
        {
            // Arrange
            var useCase = _fixture.CreateUseCase();
            var jti = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            _fixture.SetupAuthenticatedUser(jti, clientId, expiresAt);

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            // CORREÇÃO: Verifica se o método 'AddAsync' foi chamado com os dados corretos.
            _fixture.RevokedTokenRepositoryMock.Verify(
                r => r.AddAsync(jti, clientId, It.Is<DateTime>(d => d > DateTime.UtcNow), It.IsAny<CancellationToken>()), 
                Times.Once
            );
        }

        [Fact(DisplayName = "Deve Retornar Sucesso se Usuário Já Não Estiver Autenticado")]
        [Trait("Application", "Logout - UseCase")]
        public async Task Execute_WhenUserIsNotAuthenticated_ShouldSucceedWithoutAction()
        {
            // Arrange
            var useCase = _fixture.CreateUseCase();
            _fixture.SetupUnauthenticatedUser();

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _fixture.RevokedTokenRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}