using Bcommerce.Domain.Customers.Clients;
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
    }

    [Fact(DisplayName = "Deve Autenticar Cliente com Sucesso")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var client = _fixture.CreateValidClient(isEmailVerified: true); // Garante que o cliente tem e-mail verificado
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _fixture.PasswordEncripterMock
            .Setup(enc => enc.Verify(input.Password, client.PasswordHash))
            .Returns(true);

        var expectedToken = "valid_jwt_token";
        var expectedExpiration = DateTime.UtcNow.AddHours(1);
        _fixture.TokenServiceMock
            .Setup(ts => ts.GenerateToken(client))
            .Returns((expectedToken, expectedExpiration));

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(expectedToken);
        result.Value.ExpiresAt.Should().Be(expectedExpiration);

        _fixture.ClientRepositoryMock.Verify(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _fixture.PasswordEncripterMock.Verify(enc => enc.Verify(input.Password, client.PasswordHash), Times.Once);
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateToken(client), Times.Once);
    }

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
        _fixture.PasswordEncripterMock.Verify(enc => enc.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateToken(It.IsAny<Client>()), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Autenticar com Senha Incorreta")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenPasswordIsIncorrect_ShouldReturnError()
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
            .Returns(false); // Simula senha incorreta

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateToken(It.IsAny<Client>()), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Autenticar se E-mail Não Foi Verificado")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenEmailIsNotVerified_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var client = _fixture.CreateValidClient(isEmailVerified: false); // Cliente com e-mail não verificado
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada.");
        _fixture.PasswordEncripterMock.Verify(enc => enc.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateToken(It.IsAny<Client>()), Times.Never);
    }
}