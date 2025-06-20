using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Consents.Enums;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Consents;

public class Consent : Entity
{
    public Guid ClientId { get; private set; }
    public ConsentType Type { get; private set; }
    public string? TermsVersion { get; private set; }
    public bool IsGranted { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Consent() { }

    internal static Consent NewConsent(Guid clientId, ConsentType type, bool isGranted, string? termsVersion = null)
    {
        return new Consent
        {
            ClientId = clientId,
            Type = type,
            IsGranted = isGranted,
            TermsVersion = termsVersion,
            UpdatedAt = DateTime.UtcNow
        };
    }

    internal void Grant(string? termsVersion = null)
    {
        if (IsGranted) return;
        IsGranted = true;
        TermsVersion = termsVersion ?? TermsVersion;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Revoke()
    {
        if (!IsGranted) return;
        IsGranted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
}