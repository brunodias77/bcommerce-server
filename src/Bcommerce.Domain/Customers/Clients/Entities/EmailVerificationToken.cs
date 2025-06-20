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
        
    // Fábrica para ser usada pelo `ClientCreatedEventHandler`
    public static EmailVerificationToken NewToken(Guid clientId, string tokenHash, TimeSpan validityDuration)
    {
        DomainException.ThrowWhen(clientId == Guid.Empty, "ClientId é obrigatório para criar um token.");
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(tokenHash), "O hash do token é obrigatório.");

        return new EmailVerificationToken
        {
            ClientId = clientId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(validityDuration)
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public override void Validate(IValidationHandler handler)
    {
        if (ClientId == Guid.Empty)
            handler.Append(new Error("Token deve estar associado a um cliente."));
    }
}