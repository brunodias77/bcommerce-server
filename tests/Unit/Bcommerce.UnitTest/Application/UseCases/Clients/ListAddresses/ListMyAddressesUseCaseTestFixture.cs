using Bcomerce.Application.UseCases.Catalog.Clients.ListAddresses;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.ListAddresses
{
    [CollectionDefinition(nameof(ListMyAddressesUseCaseTestFixture))]
    public class ListMyAddressesUseCaseTestFixtureCollection : ICollectionFixture<ListMyAddressesUseCaseTestFixture> { }

    public class ListMyAddressesUseCaseTestFixture
    {
        public Faker Faker { get; }
        // CORREÇÃO: Mock da dependência correta
        public Mock<IAddressRepository> AddressRepositoryMock { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }

        public ListMyAddressesUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            AddressRepositoryMock = new Mock<IAddressRepository>();
            LoggedUserMock = new Mock<ILoggedUser>();
        }

        public ListMyAddressesUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando a dependência correta
            return new ListMyAddressesUseCase(
                LoggedUserMock.Object,
                AddressRepositoryMock.Object
            );
        }

        public List<Address> CreateValidAddressList(Guid clientId, int count = 3)
        {
            return Enumerable.Range(0, count).Select(_ => 
                Address.NewAddress(
                    clientId,
                    Faker.PickRandom<AddressType>(),
                    Faker.Address.ZipCode().Replace("-", ""),
                    Faker.Address.StreetName(),
                    Faker.Random.Number(100, 999).ToString(),
                    Faker.Address.SecondaryAddress(),
                    Faker.Address.StreetName(), // Bairro
                    Faker.Address.City(),
                    Faker.Address.StateAbbr(),
                    false, // Apenas um pode ser default
                    Notification.Create()
                )
            ).ToList();
        }
    }
}