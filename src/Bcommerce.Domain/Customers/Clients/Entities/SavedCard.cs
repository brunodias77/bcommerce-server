using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Entities;

public class SavedCard : Entity
{
    public Guid ClientId { get; private set; }
    public string? Nickname { get; private set; }
    public string LastFourDigits { get; private set; }
    public CardBrand Brand { get; private set; }
    public string GatewayToken { get; private set; } // O token seguro do gateway de pagamento
    public DateOnly ExpiryDate { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private SavedCard() { }

    public static SavedCard NewCard(
        Guid clientId, string lastFour, CardBrand brand,
        string gatewayToken, DateOnly expiryDate, bool isDefault, string? nickname)
    {
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(gatewayToken), "O token do gateway é obrigatório.");
        DomainException.ThrowWhen(lastFour.Length != 4, "Os últimos quatro dígitos são obrigatórios.");

        return new SavedCard
        {
            ClientId = clientId,
            LastFourDigits = lastFour,
            Brand = brand,
            GatewayToken = gatewayToken,
            ExpiryDate = expiryDate,
            IsDefault = isDefault,
            Nickname = nickname,
            CreatedAt = DateTime.UtcNow
        };
    }
        
    public bool IsExpired() => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
        
    public void SoftDelete()
    {
        if (DeletedAt.HasValue) return;
        DeletedAt = DateTime.UtcNow;
        IsDefault = false; // Um cartão deletado não pode ser o padrão
    }

    public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
}