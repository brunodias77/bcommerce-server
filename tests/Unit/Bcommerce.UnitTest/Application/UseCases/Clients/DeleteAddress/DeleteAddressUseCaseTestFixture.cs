using Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.DeleteAddress
{
    [CollectionDefinition(nameof(DeleteAddressUseCaseTestFixture))]
    public class DeleteAddressUseCaseTestFixtureCollection : ICollectionFixture<DeleteAddressUseCaseTestFixture> { }

    public class DeleteAddressUseCaseTestFixture
    {
        public Faker Faker { get; }
        // CORREÇÃO: Mock da dependência correta do UseCase.
        public Mock<IAddressRepository> AddressRepositoryMock { get; } 
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }

        public DeleteAddressUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            AddressRepositoryMock = new Mock<IAddressRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggedUserMock = new Mock<ILoggedUser>();
        }

        public DeleteAddressUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando a dependência correta.
            return new DeleteAddressUseCase(
                LoggedUserMock.Object, 
                AddressRepositoryMock.Object, 
                UnitOfWorkMock.Object
            );
        }

        public Address CreateValidAddress(Guid clientId)
        {
            // CORREÇÃO: Bug no bairro e passando o handler para o método de fábrica.
            return Address.NewAddress(
                clientId,
                AddressType.Shipping,
                Faker.Address.ZipCode().Replace("-", ""),
                Faker.Address.StreetName(),
                Faker.Random.Number(100, 999).ToString(),
                Faker.Address.SecondaryAddress(),
                Faker.Address.StreetName(), // Bairro
                Faker.Address.City(),
                Faker.Address.StateAbbr(),
                true,
                Notification.Create()
            );
        }
    }
}