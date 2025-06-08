using Bcomerce.Application.UseCases.Clients.Login;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.UnitTest.Common;
using Bogus;
using FluentAssertions;
using Moq;
using Xunit;


namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class LoginUseCaseUnitTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IPasswordEncripter> _passwordEncrypterMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginClientUseCase _useCase;
    private readonly Faker _faker = new("pt_BR");
    
    public LoginUseCaseUnitTests()
    {
        _clientRepositoryMock = new Mock<IClientRepository>();
        _passwordEncrypterMock = new Mock<IPasswordEncripter>();
        _tokenServiceMock = new Mock<ITokenService>();

        _useCase = new LoginClientUseCase(
            _clientRepositoryMock.Object,
            _passwordEncrypterMock.Object,
            _tokenServiceMock.Object
        );
    }
    
    
    [Fact(DisplayName = "Deve Retornar Sucesso e um Token com Credenciais Válidas e Verificadas")]
    public async Task Execute_WithValidAndVerifiedCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange (Organizar)
        var password = "StrongPassword123!";
        var input = new LoginClientInput(_faker.Person.Email, password);

        // Usamos nosso ClientBuilder para criar um cliente válido
        var client = ClientBuilder.New()
            .WithEmail(input.Email)
            .Build();
        
        // Importante: Simulamos que o e-mail já foi verificado
        client.VerifyEmail(); 

        // Configura o repositório para encontrar o cliente
        _clientRepositoryMock
            .Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Configura o encriptador para dizer que a senha é válida
        _passwordEncrypterMock
            .Setup(p => p.Verify(password, It.IsAny<string>()))
            .Returns(true);

        // Configura o serviço de token para retornar um token falso
        var fakeJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _tokenServiceMock
            .Setup(s => s.GenerateToken(client))
            .Returns(fakeJwt);

        // Act (Agir)
        var result = await _useCase.Execute(input);

        // Assert (Verificar)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(fakeJwt);

        // Verifica se os métodos dos mocks foram chamados
        _clientRepositoryMock.Verify(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordEncrypterMock.Verify(p => p.Verify(password, client.PasswordHash), Times.Once);
        _tokenServiceMock.Verify(s => s.GenerateToken(client), Times.Once);
    }
    
    [Fact(DisplayName = "Deve Retornar Erro de Credenciais Inválidas com Senha Incorreta")]
    public async Task Execute_WithIncorrectPassword_ShouldReturnInvalidCredentialsError()
    {
        // Arrange (Organizar)
        var password = "StrongPassword123!";
        var input = new LoginClientInput(_faker.Person.Email, password);

        // Crie um cliente válido e verificado que será "encontrado" no banco
        var client = ClientBuilder.New()
            .WithEmail(input.Email)
            .Build();
        client.VerifyEmail();

        // Configura o repositório para encontrar o cliente com sucesso
        _clientRepositoryMock
            .Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // PONTO CHAVE: Configura o encriptador para retornar 'false', simulando uma senha incorreta.
        _passwordEncrypterMock
            .Setup(p => p.Verify(password, client.PasswordHash))
            .Returns(false);

        // Act (Agir)
        var result = await _useCase.Execute(input);

        // Assert (Verificar)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    
        // Garante que a mensagem de erro é a genérica, por segurança
        result.Error.GetErrors().Should().ContainSingle()
            .Which.Message.Should().Be("E-mail ou senha inválidos.");

        // Garante que um token NUNCA foi gerado
        _tokenServiceMock.Verify(s => s.GenerateToken(It.IsAny<Client>()), Times.Never);
    }
    
    [Fact(DisplayName = "Deve Retornar Erro de Credenciais Inválidas se o Usuário Não For Encontrado")]
    public async Task Execute_WhenUserIsNotFound_ShouldReturnInvalidCredentialsError()
    {
        // Arrange (Organizar)
        var input = new LoginClientInput(_faker.Person.Email, "any_password");

        // PONTO CHAVE: Configura o repositório para retornar 'null', 
        // simulando que nenhum cliente foi encontrado com o e-mail fornecido.
        _clientRepositoryMock
            .Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);

        // Act (Agir)
        var result = await _useCase.Execute(input);

        // Assert (Verificar)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    
        // Garante que a mensagem de erro é a mesma do de senha inválida, por segurança.
        result.Error.GetErrors().Should().ContainSingle()
            .Which.Message.Should().Be("E-mail ou senha inválidos.");

        // Garante que a lógica parou antes de tentar verificar a senha ou gerar um token.
        _passwordEncrypterMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(s => s.GenerateToken(It.IsAny<Client>()), Times.Never);
    }
    
    [Fact(DisplayName = "Deve Retornar Erro se o E-mail do Cliente Não Foi Verificado")]
    public async Task Execute_WhenEmailIsNotVerified_ShouldReturnEmailNotVerifiedError()
    {
        // Arrange (Organizar)
        var password = "StrongPassword123!";
        var input = new LoginClientInput(_faker.Person.Email, password);

        // PONTO CHAVE: Crie um cliente, mas NÃO chame o método .VerifyEmail()
        // Por padrão, um novo cliente terá o campo EmailVerified como nulo.
        var client = ClientBuilder.New()
            .WithEmail(input.Email)
            .Build();

        // Configura o repositório para encontrar este cliente não verificado
        _clientRepositoryMock
            .Setup(r => r.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // A senha está correta, para garantir que estamos testando a lógica de verificação
        // e não a de senha inválida.
        _passwordEncrypterMock
            .Setup(p => p.Verify(password, client.PasswordHash))
            .Returns(true);

        // Act (Agir)
        var result = await _useCase.Execute(input);

        // Assert (Verificar)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    
        // Garante que a mensagem de erro é específica para este cenário.
        result.Error.GetErrors().Should().ContainSingle()
            .Which.Message.Should().Be("Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada.");

        // Garante que um token NUNCA foi gerado se o e-mail não foi verificado.
        _tokenServiceMock.Verify(s => s.GenerateToken(It.IsAny<Client>()), Times.Never);
    }
}