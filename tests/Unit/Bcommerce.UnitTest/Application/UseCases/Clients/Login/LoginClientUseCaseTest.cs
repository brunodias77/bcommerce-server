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
        _fixture.ConfigurationMock.Invocations.Clear();
    }

    [Fact(DisplayName = "Deve Autenticar com Sucesso e Zerar Tentativas de Login")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenCredentialsAreValid_ShouldReturnAuthResultAndResetLoginAttempts()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        // Cria um cliente que já teve falhas de login antes
        var client = _fixture.CreateValidClient(isEmailVerified: true, isLocked: false, failedAttempts: 3);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _fixture.PasswordEncripterMock
            .Setup(enc => enc.Verify(input.Password, client.PasswordHash))
            .Returns(true);
        var authResult = new AuthResult("access_token", DateTime.UtcNow.AddMinutes(15), "refresh_token");
        _fixture.TokenServiceMock
            .Setup(ts => ts.GenerateTokens(client))
            .Returns(authResult);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verifica se o método para zerar as tentativas foi chamado na entidade
        // Uma forma de verificar isso é checar se o repositório foi chamado para atualizar o cliente
        // com os valores zerados.
        _fixture.ClientRepositoryMock.Verify(
            repo => repo.Update(It.Is<Client>(c => c.FailedLoginAttempts == 0 && c.AccountLockedUntil == null), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _fixture.RefreshTokenRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Deve Incrementar Falhas de Login com Senha Incorreta")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenPasswordIsIncorrect_ShouldIncrementFailedAttempts()
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

        // Verifica se o método de atualização foi chamado para salvar a tentativa de falha
        _fixture.ClientRepositoryMock.Verify(
            repo => repo.Update(It.Is<Client>(c => c.FailedLoginAttempts == 1), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        _fixture.TokenServiceMock.Verify(ts => ts.GenerateTokens(It.IsAny<Client>()), Times.Never);
    }
    
    [Fact(DisplayName = "Deve Bloquear Conta Após Número Máximo de Tentativas")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_OnMaxFailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        // Cliente está a 1 tentativa de ser bloqueado (MaxAttempts é 5 por padrão na fixture)
        var client = _fixture.CreateValidClient(isEmailVerified: true, failedAttempts: 4);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock.Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        _fixture.PasswordEncripterMock.Setup(enc => enc.Verify(input.Password, client.PasswordHash)).Returns(false);

        // Act
        await useCase.Execute(input);

        // Assert
        // Verifica se o método de atualização foi chamado para bloquear a conta
        _fixture.ClientRepositoryMock.Verify(
            repo => repo.Update(It.Is<Client>(c => c.IsLocked == true && c.FailedLoginAttempts == 5), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Não Deve Autenticar se a Conta Estiver Bloqueada")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenAccountIsLocked_ShouldReturnLockedError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        // Cliente já está bloqueado
        var client = _fixture.CreateValidClient(isEmailVerified: true, isLocked: true);
        var useCase = _fixture.CreateUseCase();

        _fixture.ClientRepositoryMock.Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        
        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message.Contains("Sua conta está temporariamente bloqueada."));
        
        // Garante que nenhuma operação de escrita ou verificação de senha ocorreu
        _fixture.PasswordEncripterMock.Verify(enc => enc.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
    }

    // Testes para cenários de e-mail não encontrado e e-mail não verificado permanecem os mesmos,
    // pois a lógica para eles ocorre antes da verificação de senha e bloqueio.
    [Fact(DisplayName = "Não Deve Autenticar com E-mail Inexistente")]
    [Trait("Application", "LoginClient - UseCase")]
    public async Task Execute_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidLoginInput();
        var useCase = _fixture.CreateUseCase();
        _fixture.ClientRepositoryMock.Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync((Client)null);

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "E-mail ou senha inválidos.");
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
    }
}
