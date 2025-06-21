using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Customers.Clients.Repositories;

/// <summary>
/// Define o contrato para operações de persistência do agregado Client.
/// Herda as operações básicas de IRepository (Get, Insert, Update, Delete).
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>
    /// Busca um cliente ativo pelo seu endereço de e-mail.
    /// </summary>
    /// <param name="email">O e-mail a ser pesquisado.</param>
    /// <param name="cancellationToken">O token de cancelamento.</param>
    /// <returns>A entidade Client se encontrada; caso contrário, nulo.</returns>
    Task<Client?> GetByEmail(string email, CancellationToken cancellationToken);
}