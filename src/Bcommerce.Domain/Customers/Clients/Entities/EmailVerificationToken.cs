using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Entities;

public class EmailVerificationToken : Entity
{
    public Guid ClientId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private EmailVerificationToken() { }
        
    public static EmailVerificationToken NewToken(Guid clientId, string tokenHash, TimeSpan validityDuration)
    {
        DomainException.ThrowWhen(clientId == Guid.Empty, "ClientId é obrigatório para criar um token.");
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(tokenHash), "O hash do token é obrigatório.");

        var token = new EmailVerificationToken
        {
            // Id é gerado automaticamente pelo construtor da classe base 'Entity'
            ClientId = clientId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(validityDuration)
        };
        token.Validate(Bcommerce.Domain.Validation.Handlers.Notification.Create());
        return token;
    }

    // MÉTODO 'WITH' ADICIONADO PARA HIDRATAÇÃO
    public static EmailVerificationToken With(Guid id, Guid clientId, string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        return new EmailVerificationToken
        {
            Id = id,
            ClientId = clientId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public override void Validate(IValidationHandler handler)
    {
        if (ClientId == Guid.Empty)
            handler.Append(new Error("Token deve estar associado a um cliente."));
        if (Id == Guid.Empty)
            handler.Append(new Error("Token deve ter um Id."));
    }
}