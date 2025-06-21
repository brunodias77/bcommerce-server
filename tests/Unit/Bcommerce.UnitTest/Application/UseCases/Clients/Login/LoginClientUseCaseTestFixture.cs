using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
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
    
    // --> NOVOS MOCKS ADICIONADOS
    public Mock<IRefreshTokenRepository> RefreshTokenRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public LoginClientUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        ClientRepositoryMock = new Mock<IClientRepository>();
        PasswordEncripterMock = new Mock<IPasswordEncripter>();
        TokenServiceMock = new Mock<ITokenService>();
        
        // --> INICIALIZAÇÃO DOS NOVOS MOCKS
        RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }
    
    public LoginClientInput GetValidLoginInput()
    {
        return new LoginClientInput(
            Faker.Internet.Email(),
            Faker.Internet.Password(10)
        );
    }
    
    public Client CreateValidClient(bool isEmailVerified = true)
    {
        var client = Client.NewClient(
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            Faker.Phone.PhoneNumber(),
            "valid_hashed_password",
            null, null, false,
            Notification.Create()
        );

        if (isEmailVerified)
        {
            client.VerifyEmail();
        }
        
        client.ClearEvents();
        return client;
    }

    /// <summary>
    /// Cria uma instância do LoginClientUseCase com suas dependências "mockadas".
    /// </summary>
    public LoginClientUseCase CreateUseCase()
    {
        // --> CONSTRUTOR ATUALIZADO COM OS NOVOS MOCKS
        return new LoginClientUseCase(
            ClientRepositoryMock.Object,
            PasswordEncripterMock.Object,
            TokenServiceMock.Object,
            RefreshTokenRepositoryMock.Object,
            UnitOfWorkMock.Object
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
