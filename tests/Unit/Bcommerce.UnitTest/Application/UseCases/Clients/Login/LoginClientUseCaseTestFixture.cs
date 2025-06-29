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
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Login
{
    [CollectionDefinition(nameof(LoginClientUseCaseTestFixture))]
    public class LoginClientUseCaseTestFixtureCollection : ICollectionFixture<LoginClientUseCaseTestFixture> { }

    public class LoginClientUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IClientRepository> ClientRepositoryMock { get; }
        public Mock<IPasswordEncripter> PasswordEncripterMock { get; }
        public Mock<ITokenService> TokenServiceMock { get; }
        // CORREÇÃO: Mocks para TODAS as dependências do UseCase
        public Mock<IRefreshTokenRepository> RefreshTokenRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IConfiguration> ConfigurationMock { get; }

        public LoginClientUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ClientRepositoryMock = new Mock<IClientRepository>();
            PasswordEncripterMock = new Mock<IPasswordEncripter>();
            TokenServiceMock = new Mock<ITokenService>();
            RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            ConfigurationMock = new Mock<IConfiguration>();

            // Configuração padrão para os mocks de configuração
            SetupConfiguration("Settings:AccountLockout:MaxFailedAccessAttempts", "5");
            SetupConfiguration("Settings:AccountLockout:DefaultLockoutMinutes", "15");
        }

        public LoginClientUseCase CreateUseCase()
        {
            return new LoginClientUseCase(
                ClientRepositoryMock.Object,
                PasswordEncripterMock.Object,
                TokenServiceMock.Object,
                RefreshTokenRepositoryMock.Object,
                UnitOfWorkMock.Object,
                ConfigurationMock.Object
            );
        }

        public LoginClientInput GetValidLoginInput()
        {
            return new LoginClientInput(Faker.Internet.Email(), "valid_password");
        }

        public Client CreateValidClient(bool isEmailVerified = true, bool isLocked = false, int failedAttempts = 0)
        {
            var client = Client.With(
                System.Guid.NewGuid(), Faker.Name.FirstName(), Faker.Name.LastName(),
                Faker.Internet.Email(),
                isEmailVerified ? System.DateTime.UtcNow.AddDays(-1) : null,
                Faker.Phone.PhoneNumber(), "hashed_password", null, null, false,
                ClientStatus.Active, Role.Customer, failedAttempts,
                isLocked ? System.DateTime.UtcNow.AddMinutes(15) : null,
                System.DateTime.UtcNow.AddDays(-10), System.DateTime.UtcNow.AddDays(-1), null
            );
            client.ClearEvents();
            return client;
        }
        
        // Método auxiliar para facilitar a configuração do IConfiguration
        private void SetupConfiguration(string key, string value)
        {
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns(value);
            ConfigurationMock.Setup(c => c.GetSection(key)).Returns(configSection.Object);
        }
    }
}