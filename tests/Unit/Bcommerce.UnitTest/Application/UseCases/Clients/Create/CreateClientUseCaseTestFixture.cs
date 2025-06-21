using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Create;

/// <summary>
/// Fixture para os testes do CreateClientUseCase.
/// Responsável por configurar todos os mocks das dependências e gerar dados de teste,
/// permitindo que os testes sejam limpos e focados na lógica do caso de uso.
/// </summary>
public class CreateClientUseCaseTestFixture
{
    public Faker Faker { get; }
    
    // Mocks para todas as dependências injetadas no UseCase
    public Mock<IClientRepository> ClientRepositoryMock { get; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; }
    public Mock<IPasswordEncripter> PasswordEncripterMock { get; }
    public Mock<IDomainEventPublisher> DomainEventPublisherMock { get; }
    public Mock<ILogger<CreateClientUseCase>> LoggerMock { get; }
    
    public CreateClientUseCaseTestFixture()
    {
        Faker = new Faker("pt_BR");
        ClientRepositoryMock = new Mock<IClientRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        PasswordEncripterMock = new Mock<IPasswordEncripter>();
        DomainEventPublisherMock = new Mock<IDomainEventPublisher>();
        LoggerMock = new Mock<ILogger<CreateClientUseCase>>();
    }
    
    /// <summary>
    /// Gera um input válido para o CreateClientUseCase.
    /// </summary>
    public CreateClientInput GetValidCreateClientInput()
    {
        return new CreateClientInput(
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            Faker.Phone.PhoneNumber(),
            Faker.Internet.Password(8), // Senha com no mínimo 8 caracteres
            Faker.Random.Bool()
        );
    }

    /// <summary>
    /// Cria uma instância válida da entidade Client.
    /// É usado para simular o retorno do repositório quando um cliente já existe.
    /// </summary>
    /// <returns>Uma instância válida de <see cref="Client"/>.</returns>
    public Client CreateValidClient()
    {
        var input = GetValidCreateClientInput();
        return Client.NewClient(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            "any_hash",
            null, null, false,
            Notification.Create()
        );
    }
    
    /// <summary>
    /// Cria uma instância do CreateClientUseCase com todas as suas dependências "mockadas".
    /// </summary>
    /// <returns>Uma instância de <see cref="CreateClientUseCase"/> pronta para ser testada.</returns>
    public CreateClientUseCase CreateUseCase()
    {
        return new CreateClientUseCase(
            ClientRepositoryMock.Object,
            UnitOfWorkMock.Object,
            PasswordEncripterMock.Object,
            LoggerMock.Object,
            DomainEventPublisherMock.Object
        );
    }
}

/// <summary>
/// Definição da coleção para a fixture, garantindo que a mesma instância
/// da fixture seja compartilhada entre todos os testes da coleção.
/// </summary>
[CollectionDefinition(nameof(CreateClientUseCaseTestFixture))]
public class CreateClientUseCaseTestFixtureCollection : ICollectionFixture<CreateClientUseCaseTestFixture>
{
}


