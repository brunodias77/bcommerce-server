using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Login;

/// <summary>
/// Fixture para os testes do LoginClientUseCase.
/// Centraliza a configuração de mocks e a geração de dados de teste.
/// </summary>
public class LoginClientUseCaseTestFixture
{
    public Faker Faker { get; }
    
    // Mocks para as dependências do LoginClientUseCase
    public Mock<IClientRepository> ClientRepositoryMock { get; }
    public Mock<IPasswordEncripter> PasswordEncripterMock { get; }
    public Mock<ITokenService> TokenServiceMock { get; }
    public Mock<IRefreshTokenRepository> RefreshTokenRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }
    
    // --> NOVO MOCK ADICIONADO
    public Mock<IConfiguration> ConfigurationMock { get; }

    public LoginClientUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        ClientRepositoryMock = new Mock<IClientRepository>();
        PasswordEncripterMock = new Mock<IPasswordEncripter>();
        TokenServiceMock = new Mock<ITokenService>();
        RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        
        // --> INICIALIZAÇÃO DO NOVO MOCK
        ConfigurationMock = new Mock<IConfiguration>();
        
        // Configura valores padrão para as configurações de lockout
        SetupConfiguration("Settings:AccountLockout:MaxFailedAccessAttempts", "5");
        SetupConfiguration("Settings:AccountLockout:DefaultLockoutMinutes", "15");
    }
    
    public LoginClientInput GetValidLoginInput()
    {
        return new LoginClientInput(
            Faker.Internet.Email(),
            Faker.Internet.Password(10)
        );
    }
    
    // --> MÉTODO ATUALIZADO PARA SUPORTAR O ESTADO DE BLOQUEIO
    public Client CreateValidClient(bool isEmailVerified = true, bool isLocked = false, int failedAttempts = 0)
    {
        var client = Client.With(
            Guid.NewGuid(),
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            isEmailVerified ? DateTime.UtcNow : null,
            Faker.Phone.PhoneNumber(),
            "valid_hashed_password",
            null,
            null,
            false,
            ClientStatus.Active,
            Role.Customer,
            failedAttempts,
            isLocked ? DateTime.UtcNow.AddMinutes(15) : null,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-1),
            null
        );
        
        client.ClearEvents();
        return client;
    }

    // Método auxiliar para configurar o mock de IConfiguration
    public void SetupConfiguration(string key, string value)
    {
        ConfigurationMock.Setup(c => c.GetSection(key).Value).Returns(value);
    }

    /// <summary>
    /// Cria uma instância do LoginClientUseCase com suas dependências "mockadas".
    /// </summary>
    public LoginClientUseCase CreateUseCase()
    {
        return new LoginClientUseCase(
            ClientRepositoryMock.Object,
            PasswordEncripterMock.Object,
            TokenServiceMock.Object,
            RefreshTokenRepositoryMock.Object,
            UnitOfWorkMock.Object,
            ConfigurationMock.Object // <-- MOCK DE CONFIGURATION ADICIONADO
        );
    }
}

/// <summary>
/// Definição da coleção para a fixture de teste do LoginClientUseCase.
/// </summary>
[CollectionDefinition(nameof(LoginClientUseCaseTestFixture))]
public class LoginClientUseCaseTestFixtureCollection : ICollectionFixture<LoginClientUseCaseTestFixture>
{
}
