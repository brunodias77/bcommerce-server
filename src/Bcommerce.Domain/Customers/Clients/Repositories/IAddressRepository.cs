using Bcommerce.Domain.Customers.Clients.Entities;

namespace Bcommerce.Domain.Customers.Clients.Repositories;

/// <summary>
/// Define o contrato para o repositório da entidade Address.
/// </summary>
public interface IAddressRepository
{
    /// <summary>
    /// Adiciona um novo endereço ao repositório.
    /// </summary>
    /// <param name="address">A entidade de endereço a ser adicionada.</param>
    /// <param name="cancellationToken">O token para cancelamento da operação.</param>
    Task AddAsync(Address address, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém um endereço pelo seu identificador único.
    /// </summary>
    /// <param name="addressId">O ID do endereço.</param>
    /// <param name="cancellationToken">O token para cancelamento da operação.</param>
    /// <returns>A entidade Address se encontrada; caso contrário, nulo.</returns>
    Task<Address?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém todos os endereços associados a um cliente específico.
    /// </summary>
    /// <param name="clientId">O ID do cliente.</param>
    /// <param name="cancellationToken">O token para cancelamento da operação.</param>
    /// <returns>Uma coleção de endereços do cliente.</returns>
    Task<IEnumerable<Address>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Atualiza um endereço existente no repositório.
    /// </summary>
    /// <param name="address">A entidade de endereço com os dados atualizados.</param>
    /// <param name="cancellationToken">O token para cancelamento da operação.</param>
    Task UpdateAsync(Address address, CancellationToken cancellationToken);
}