using Bcomerce.Application.UseCases.Catalog.Clients.RefreshToken;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities; // Adicionado para visibilidade direta
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.RefreshToken
{
    [CollectionDefinition(nameof(RefreshTokenUseCaseTestFixture))]
    public class RefreshTokenUseCaseTestFixtureCollection : ICollectionFixture<RefreshTokenUseCaseTestFixture> { }

    public class RefreshTokenUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IClientRepository> ClientRepositoryMock { get; }
        public Mock<IRefreshTokenRepository> RefreshTokenRepositoryMock { get; }
        public Mock<ITokenService> TokenServiceMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public RefreshTokenUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ClientRepositoryMock = new Mock<IClientRepository>();
            RefreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            TokenServiceMock = new Mock<ITokenService>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public RefreshTokenUseCase CreateUseCase()
        {
            return new RefreshTokenUseCase(
                ClientRepositoryMock.Object,
                RefreshTokenRepositoryMock.Object,
                TokenServiceMock.Object,
                UnitOfWorkMock.Object
            );
        }

        public RefreshTokenInput GetValidInput() => new(Guid.NewGuid().ToString("N"));

        public Client CreateValidClient()
        {
            // CORREÇÃO: Adicionando o parâmetro 'handler' que faltava na chamada.
            return Client.NewClient(
                Faker.Name.FirstName(), Faker.Name.LastName(), Faker.Internet.Email(),
                Faker.Phone.PhoneNumber(), "hashed_password", null, null, false, 
                Notification.Create()
            );
        }

        public Bcommerce.Domain.Customers.Clients.Entities.RefreshToken CreateValidRefreshToken(Guid clientId, bool isActive = true)
        {
            var token = Bcommerce.Domain.Customers.Clients.Entities.RefreshToken.NewToken(clientId, Guid.NewGuid().ToString("N"), TimeSpan.FromDays(7));
            if (!isActive)
            {
                token.Revoke(); 
            }
            return token;
        }
    }
}