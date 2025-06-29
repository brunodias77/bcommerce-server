using Bcomerce.Application.UseCases.Catalog.Clients.UpdateAddress;
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

namespace Bcommerce.UnitTest.Application.UseCases.Clients.UpdateAddress
{
    [CollectionDefinition(nameof(UpdateAddressUseCaseTestFixture))]
    public class UpdateAddressUseCaseTestFixtureCollection : ICollectionFixture<UpdateAddressUseCaseTestFixture> { }

    public class UpdateAddressUseCaseTestFixture
    {
        public Faker Faker { get; }
        // CORREÇÃO: Mocks das dependências corretas
        public Mock<IAddressRepository> AddressRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }

        public UpdateAddressUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            AddressRepositoryMock = new Mock<IAddressRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggedUserMock = new Mock<ILoggedUser>();
        }

        public UpdateAddressUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando as dependências corretas
            return new UpdateAddressUseCase(
                LoggedUserMock.Object, 
                AddressRepositoryMock.Object, 
                UnitOfWorkMock.Object
            );
        }

        public UpdateAddressInput GetValidInput(Guid addressId)
        {
            // CORREÇÃO: Gerando o record 'UpdateAddressInput' "plano", sem payload aninhado.
            return new UpdateAddressInput(
                addressId,
                Type: Faker.PickRandom<AddressType>(),
                PostalCode: Faker.Address.ZipCode().Replace("-", ""),
                Street: Faker.Address.StreetName(),
                StreetNumber: Faker.Random.Number(1000, 2000).ToString(),
                Complement: "Updated Complement",
                Neighborhood: Faker.Address.City(), // Bairro
                City: Faker.Address.City(),
                StateCode: Faker.Address.StateAbbr(),
                IsDefault: false
            );
        }

        public Address CreateValidAddress(Guid clientId)
        {
            return Address.NewAddress(
                clientId, AddressType.Shipping, "12345678", "Old Street", "123", 
                null, "Old Neighborhood", "Old City", "OS", true, Notification.Create()
            );
        }
    }
}