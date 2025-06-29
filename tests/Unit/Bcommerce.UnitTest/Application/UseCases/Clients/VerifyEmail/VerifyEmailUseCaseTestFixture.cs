using Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.VerifyEmail
{
    [CollectionDefinition(nameof(VerifyEmailUseCaseTestFixture))]
    public class VerifyEmailUseCaseTestFixtureCollection : ICollectionFixture<VerifyEmailUseCaseTestFixture> { }

    public class VerifyEmailUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IClientRepository> ClientRepositoryMock { get; }
        public Mock<IEmailVerificationTokenRepository> TokenRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        // CORREÇÃO: Adicionando o mock do Logger
        public Mock<ILogger<VerifyEmailUseCase>> LoggerMock { get; }

        public VerifyEmailUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ClientRepositoryMock = new Mock<IClientRepository>();
            TokenRepositoryMock = new Mock<IEmailVerificationTokenRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggerMock = new Mock<ILogger<VerifyEmailUseCase>>();
        }

        public VerifyEmailUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando todas as dependências
            return new VerifyEmailUseCase(
                ClientRepositoryMock.Object,
                TokenRepositoryMock.Object,
                UnitOfWorkMock.Object,
                LoggerMock.Object
            );
        }

        public Client CreateValidClient()
        {
            return Client.NewClient(
                Faker.Name.FirstName(), Faker.Name.LastName(), Faker.Internet.Email(),
                Faker.Phone.PhoneNumber(), "hashed_password", null, null, false, Notification.Create()
            );
        }

        public EmailVerificationToken CreateValidTokenEntity(Guid clientId, bool isExpired = false)
        {
            // CORREÇÃO: Criando a entidade de domínio, não o DataModel.
            var validity = isExpired ? TimeSpan.FromHours(-1) : TimeSpan.FromHours(1);
            return EmailVerificationToken.NewToken(clientId, Guid.NewGuid().ToString(), validity);
        }
    }
}