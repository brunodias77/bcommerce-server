using Bcommerce.Domain.Customers.Clients;

namespace Bcommerce.Domain.Services;

public interface ITokenService
{
    /// <summary>
    /// Gera um token baseado nos dados de um cliente.
    /// </summary>
    /// <param name="client">A entidade do cliente.</param>
    /// <returns>Uma string representando o token JWT.</returns>
    string GenerateToken(Client client);
}