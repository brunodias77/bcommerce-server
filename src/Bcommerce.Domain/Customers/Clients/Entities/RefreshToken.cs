using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Entities;

public class RefreshToken : Entity
{
    public Guid ClientId { get; private set; }
    public string TokenValue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken NewToken(Guid clientId, string tokenValue, TimeSpan validity)
    {
        DomainException.ThrowWhen(clientId == Guid.Empty, "ClientId é obrigatório.");
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(tokenValue), "O valor do token é obrigatório.");

        return new RefreshToken
        {
            ClientId = clientId,
            TokenValue = tokenValue,
            ExpiresAt = DateTime.UtcNow.Add(validity),
            CreatedAt = DateTime.UtcNow,
        };
    }
    
    // Método 'With' para hidratação pelo repositório
    public static RefreshToken With(Guid id, Guid clientId, string tokenValue, DateTime expiresAt, DateTime createdAt, DateTime? revokedAt)
    {
        return new RefreshToken
        {
            Id = id,
            ClientId = clientId,
            TokenValue = tokenValue,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            RevokedAt = revokedAt
        };
    }

    public void Revoke()
    {
        if (RevokedAt.HasValue) return; // Já foi revogado
        RevokedAt = DateTime.UtcNow;
    }

    public override void Validate(IValidationHandler handler) { }
}