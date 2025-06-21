using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Services;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Login;

[Collection(nameof(LoginClientUseCaseTestFixture))]
public class LoginClientUseCaseTest
{
    private readonly LoginClientUseCaseTestFixture _fixture;

    public LoginClientUseCaseTest(LoginClientUseCaseTestFixture fixture)
    {
        _fixture = fixture;
        // Limpa as invocações dos mocks antes de cada teste para garantir isolamento
        _fixture.ClientRepositoryMock.Invocations.Clear();
        _fixture.PasswordEncripterMock.Invocations.Clear();
        _fixture.TokenServiceMock.Invocations.Clear();
        _fixture.RefreshTokenRepositoryMock.Invocations.Clear();
        _fixture.UnitOfWorkMock.Invocations.Clear();
    }

    [Fact(DisplayName = "Deve Autenticar Cliente com Sucesso e Gerar Tokens")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenCredentialsAreValid_ShouldReturnAuthResultAndSaveRefreshToken()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var client = _fixture.CreateValidClient(isEmailVerified: true);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _fixture.PasswordEncripterMock
            .Setup(enc => enc.Verify(input.Password, client.PasswordHash))
            .Returns(true);

        // --> SETUP ATUALIZADO: Agora o TokenService retorna um AuthResult
        var authResult = new AuthResult(
            "valid_access_token",
            DateTime.UtcNow.AddMinutes(15),
            "valid_refresh_token"
        );
        _fixture.TokenServiceMock
            .Setup(ts => ts.GenerateTokens(client))
            .Returns(authResult);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(authResult.AccessToken);
        result.Value.RefreshToken.Should().Be(authResult.RefreshToken); // <-- Novo Assert

        // --> NOVAS VERIFICAÇÕES
        _fixture.RefreshTokenRepositoryMock.Verify(
            repo => repo.AddAsync(
                It.Is<RefreshToken>(rt => rt.ClientId == client.Id && rt.TokenValue == authResult.RefreshToken),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);

        // Verifica se os mocks antigos ainda são chamados corretamente
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateTokens(client), Times.Once);
    }

    // Os testes de falha agora também precisam verificar se as operações de escrita (Commit/AddAsync) NÃO foram chamadas.

    [Fact(DisplayName = "Não Deve Autenticar com E-mail Inexistente")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");
        
        // --> VERIFICAÇÃO ADICIONAL
        _fixture.RefreshTokenRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Autenticar com Senha Incorreta")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenPasswordIsIncorrect_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var client = _fixture.CreateValidClient(isEmailVerified: true);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock.Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        _fixture.PasswordEncripterMock.Setup(enc => enc.Verify(input.Password, client.PasswordHash)).Returns(false);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");

        // --> VERIFICAÇÃO ADICIONAL
        _fixture.RefreshTokenRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Autenticar se E-mail Não Foi Verificado")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenEmailIsNotVerified_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var client = _fixture.CreateValidClient(isEmailVerified: false);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock.Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada.");
        
        // --> VERIFICAÇÃO ADICIONAL
        _fixture.RefreshTokenRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
    }
}