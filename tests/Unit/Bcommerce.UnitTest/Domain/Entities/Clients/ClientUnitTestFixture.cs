using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;

namespace Bcommerce.UnitTest.Domain.Entities.Clients;

/// <summary>
/// Fixture de teste para a entidade Client.
/// Uma fixture é um objeto que é criado apenas uma vez por classe de teste (ou coleção).
/// Ela é usada para compartilhar um contexto e dados comuns entre todos os testes,
/// evitando repetição de código e melhorando o desempenho.
/// </summary>
public class ClientTestFixture
{
    public Faker Faker { get; }

    public ClientTestFixture()
    {
        // A instância do Faker é criada aqui, uma única vez para todos os testes
        // que usarem esta fixture.
        Faker = new Faker("pt_BR");
    }

    /// <summary>
    /// Gera um conjunto de dados de entrada válidos para a criação de um cliente.
    /// </summary>
    /// <returns>Uma tupla com dados válidos para um cliente.</returns>
    public (string FirstName, string LastName, string Email, string PhoneNumber, string PasswordHash) GetValidClientInputData()
    {
        return (
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            Faker.Phone.PhoneNumber(),
            "any_valid_password_hash"
        );
    }

    /// <summary>
    /// Cria e retorna uma instância válida da entidade Client, pronta para ser usada nos testes.
    /// </summary>
    /// <returns>Uma instância válida de <see cref="Client"/>.</returns>
    public Client CreateValidClient()
    {
        var (firstName, lastName, email, phone, passwordHash) = GetValidClientInputData();
        
        var client = Client.NewClient(
            firstName,
            lastName,
            email,
            phone,
            passwordHash,
            null,
            null,
            false,
            Notification.Create()
        );

        // Limpamos o evento de criação para não interferir em testes de outros eventos.
        client.ClearEvents();
        
        return client;
    }
}

/// <summary>
/// Esta classe vazia serve como uma definição de coleção para o xUnit.
/// Ela informa ao xUnit que todos os testes marcados com [Collection("ClientTests")]
/// devem compartilhar a mesma instância da ClientTestFixture, garantindo que
/// a fixture seja criada apenas uma vez para o conjunto de testes.
/// </summary>
[CollectionDefinition("ClientTests")]
public class ClientTestFixtureCollection : ICollectionFixture<ClientTestFixture>
{
}