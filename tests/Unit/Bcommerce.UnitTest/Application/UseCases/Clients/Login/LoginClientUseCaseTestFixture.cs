using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
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

    public LoginClientUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        ClientRepositoryMock = new Mock<IClientRepository>();
        PasswordEncripterMock = new Mock<IPasswordEncripter>();
        TokenServiceMock = new Mock<ITokenService>();
    }

    /// <summary>
    /// Gera um input de login válido.
    /// </summary>
    public LoginClientInput GetValidLoginInput()
    {
        return new LoginClientInput(
            Faker.Internet.Email(),
            Faker.Internet.Password(10)
        );
    }

    /// <summary>
    /// Cria uma instância de um cliente válido para ser retornado pelos mocks.
    /// </summary>
    /// <param name="isEmailVerified">Define se o cliente retornado deve ter o e-mail verificado.</param>
    /// <returns>Uma instância válida de Client.</returns>
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
        return new LoginClientUseCase(
            ClientRepositoryMock.Object,
            PasswordEncripterMock.Object,
            TokenServiceMock.Object
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
