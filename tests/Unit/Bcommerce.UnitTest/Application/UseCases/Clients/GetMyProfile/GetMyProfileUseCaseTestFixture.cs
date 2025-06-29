using Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.GetMyProfile
{
    [CollectionDefinition(nameof(GetMyProfileUseCaseTestFixture))]
    public class GetMyProfileUseCaseTestFixtureCollection : ICollectionFixture<GetMyProfileUseCaseTestFixture> { }

    public class GetMyProfileUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<IClientRepository> ClientRepositoryMock { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }

        public GetMyProfileUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            ClientRepositoryMock = new Mock<IClientRepository>();
            LoggedUserMock = new Mock<ILoggedUser>();
        }

        public GetMyProfileUseCase CreateUseCase()
        {
            return new GetMyProfileUseCase(ClientRepositoryMock.Object, LoggedUserMock.Object);
        }

        public Client CreateValidClient()
        {
            return Client.NewClient(
                Faker.Name.FirstName(),
                Faker.Name.LastName(),
                Faker.Internet.Email(),
                Faker.Phone.PhoneNumber(),
                "valid_password_hash",
                null, null, false,
                Notification.Create()
            );
        }
    }
}