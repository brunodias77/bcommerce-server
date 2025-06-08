namespace Bcommerce.Domain.Clients.Repositories;

public interface IAddressRepository
{
    /// <summary>
    /// Adiciona um novo endereço ao banco de dados.
    /// </summary>
    Task AddAsync(Address address, CancellationToken cancellationToken);

    /// <summary>
    /// Busca um endereço específico pelo seu ID.
    /// </summary>
    Task<Address?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken);

    /// <summary>
    /// Busca todos os endereços associados a um cliente específico.
    /// </summary>
    Task<IEnumerable<Address>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Atualiza um endereço existente no banco de dados.
    /// </summary>
    /// <remarks>
    /// Este método também será usado para persistir o soft delete, 
    /// já que a lógica de negócio (definir o `DeletedAt`) ocorre na entidade.
    /// </remarks>
    Task UpdateAsync(Address address, CancellationToken cancellationToken);
}