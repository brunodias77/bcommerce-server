using Bcommerce.Domain.Clients;

namespace Bcomerce.Application.UseCases.Clients.Create;

public record CreateClientOutput(
    Guid Id, 
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Cria uma instância de CreateClientOutput a partir de uma entidade Client.
    /// </summary>
    /// <param name="client">A entidade Client a ser mapeada.</param>
    /// <returns>Uma nova instância de CreateClientOutput.</returns>
    /// <exception cref="ArgumentNullException">Se o cliente fornecido for nulo.</exception>
    public static CreateClientOutput FromClient(Client client)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        return new CreateClientOutput(
            client.Id,
            client.FirstName,
            client.LastName,
            client.Email,
            client.CreatedAt
        );
    }
}