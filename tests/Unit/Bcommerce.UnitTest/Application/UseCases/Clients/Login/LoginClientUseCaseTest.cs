using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Services;
using FluentAssertions;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Login
{
    [Collection(nameof(LoginClientUseCaseTestFixture))]
    public class LoginClientUseCaseTest
    {
        private readonly LoginClientUseCaseTestFixture _fixture;

        public LoginClientUseCaseTest(LoginClientUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ClientRepositoryMock.Invocations.Clear();
            _fixture.PasswordEncripterMock.Invocations.Clear();
            _fixture.TokenServiceMock.Invocations.Clear();
            _fixture.RefreshTokenRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Logar com Credenciais Válidas")]
        [Trait("Application", "LoginClient - UseCase")]
        public async Task Execute_WhenCredentialsAreValid_ShouldReturnTokensAndCommit()
        {
            // Arrange
            var input = _fixture.GetValidLoginInput();
            var client = _fixture.CreateValidClient(isEmailVerified: true, failedAttempts: 2); // Cliente com falhas anteriores
            var useCase = _fixture.CreateUseCase();
            var authResult = new AuthResult("access_token", System.DateTime.UtcNow.AddMinutes(15), "refresh_token");

            _fixture.ClientRepositoryMock.Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.PasswordEncripterMock.Setup(p => p.Verify(input.Password, client.PasswordHash)).Returns(true);
            _fixture.TokenServiceMock.Setup(t => t.GenerateTokens(client)).Returns(authResult);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.AccessToken.Should().Be("access_token");
            
            // Verifica se o login resetou as tentativas de falha
            _fixture.ClientRepositoryMock.Verify(r => r.Update(It.Is<Client>(c => c.FailedLoginAttempts == 0), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.RefreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Bcommerce.Domain.Customers.Clients.Entities.RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Logar com Senha Inválida")]
        [Trait("Application", "LoginClient - UseCase")]
        public async Task Execute_WhenPasswordIsInvalid_ShouldReturnErrorAndIncrementAttempts()
        {
            // Arrange
            var input = _fixture.GetValidLoginInput();
            var client = _fixture.CreateValidClient(isEmailVerified: true);
            var useCase = _fixture.CreateUseCase();

            _fixture.ClientRepositoryMock.Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.PasswordEncripterMock.Setup(p => p.Verify(input.Password, client.PasswordHash)).Returns(false);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");
            _fixture.ClientRepositoryMock.Verify(r => r.Update(It.Is<Client>(c => c.FailedLoginAttempts == 1), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Logar com Usuário Inexistente")]
        [Trait("Application", "LoginClient - UseCase")]
        public async Task Execute_WhenUserNotFound_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidLoginInput();
            var useCase = _fixture.CreateUseCase();

            _fixture.ClientRepositoryMock.Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync((Client)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
        
        [Fact(DisplayName = "Não Deve Logar se Email Não Foi Verificado")]
        [Trait("Application", "LoginClient - UseCase")]
        public async Task Execute_WhenEmailIsNotVerified_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidLoginInput();
            var client = _fixture.CreateValidClient(isEmailVerified: false); // Email não verificado
            var useCase = _fixture.CreateUseCase();

            _fixture.ClientRepositoryMock.Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada.");
        }
        
        [Fact(DisplayName = "Não Deve Logar se Conta Estiver Bloqueada")]
        [Trait("Application", "LoginClient - UseCase")]
        public async Task Execute_WhenAccountIsLocked_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidLoginInput();
            var client = _fixture.CreateValidClient(isLocked: true); // Conta bloqueada
            var useCase = _fixture.CreateUseCase();

            _fixture.ClientRepositoryMock.Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            
            // Act
            var result = await useCase.Execute(input);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().First().Message.Should().Contain("Sua conta está temporariamente bloqueada.");
        }
    }
}