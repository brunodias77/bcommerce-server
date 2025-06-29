using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.AddAddress
{
    [CollectionDefinition(nameof(AddAddressUseCaseTestFixture))]
    public class AddAddressUseCaseTestFixtureCollection : ICollectionFixture<AddAddressUseCaseTestFixture> { }

    public class AddAddressUseCaseTestFixture
    {
        public Faker Faker { get; }
        // CORREÇÃO: Mock da dependência correta
        public Mock<IAddressRepository> AddressRepositoryMock { get; } 
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<ILoggedUser> LoggedUserMock { get; }

        public AddAddressUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            AddressRepositoryMock = new Mock<IAddressRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            LoggedUserMock = new Mock<ILoggedUser>();
        }

        public AddAddressUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando a dependência correta
            return new AddAddressUseCase(
                LoggedUserMock.Object, 
                AddressRepositoryMock.Object, 
                UnitOfWorkMock.Object
            );
        }

        public AddAddressInput GetValidInput()
        {
            // CORREÇÃO: Usando o Enum e as propriedades corretas do record
            return new AddAddressInput(
                Type: Faker.PickRandom<AddressType>(),
                PostalCode: Faker.Address.ZipCode().Replace("-", ""),
                Street: Faker.Address.StreetName(),
                StreetNumber: Faker.Random.Number(100, 999).ToString(),
                Complement: Faker.Address.SecondaryAddress(),
                Neighborhood: Faker.Address.City(), // Bairro
                City: Faker.Address.City(),
                StateCode: Faker.Address.StateAbbr(),
                IsDefault: Faker.Random.Bool()
            );
        }
    }
}