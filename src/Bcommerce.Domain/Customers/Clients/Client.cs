using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Events;
using Bcommerce.Domain.Customers.Clients.Validators;
using Bcommerce.Domain.Customers.Clients.ValueObjects;
using Bcommerce.Domain.Customers.Consents;
using Bcommerce.Domain.Customers.Consents.Enums;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients;

public class Client : AggregateRoot
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public DateTime? EmailVerified { get; private set; }
    public string PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public Cpf? Cpf { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public bool NewsletterOptIn { get; private set; }
    public ClientStatus Status { get; private set; }
    public Role Role { get; private set; } 
    public int FailedLoginAttempts { get; private set; }
    public DateTime? AccountLockedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    private readonly List<SavedCard> _savedCards = new();
    public IReadOnlyCollection<SavedCard> SavedCards => _savedCards.AsReadOnly();
    private readonly List<Consent> _consents = new();
    public IReadOnlyCollection<Consent> Consents => _consents.AsReadOnly();
    
    public bool IsLocked => AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;
    
    private Client() : base() { }

    public static Client NewClient(
        string firstName, string lastName, string email, string phoneNumber,
        string passwordHash, string? cpf, DateOnly? dateOfBirth, bool newsletterOptIn,
        IValidationHandler handler)
    {
        var client = new Client
        {
            FirstName = firstName,
            LastName = lastName,
            Email = new Email(email, handler),
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Cpf = cpf is not null ? new Cpf(cpf, handler) : null,
            DateOfBirth = dateOfBirth,
            Role = Role.Customer,
            NewsletterOptIn = newsletterOptIn,
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        client.Validate(handler);
        if (!handler.HasError())
        {
            client.RaiseEvent(new ClientCreatedEvent(client.Id, client.FirstName, client.Email.Value));
        }

        return client;
    }

    public static Client With(
        Guid id, string firstName, string lastName, string email, DateTime? emailVerified,
        string phone, string passwordHash, string? cpf, DateOnly? dateOfBirth,
        bool newsletterOptIn, ClientStatus status, Role role, int failedLoginAttempts, DateTime? accountLockedUntil, 
        DateTime createdAt, DateTime updatedAt, DateTime? deletedAt)
    {
        var client = new Client
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = Email.With(email),
            EmailVerified = emailVerified,
            PhoneNumber = phone,
            PasswordHash = passwordHash,
            Cpf = cpf is not null ? Cpf.With(cpf) : null,
            DateOfBirth = dateOfBirth,
            NewsletterOptIn = newsletterOptIn,
            Status = status,
            FailedLoginAttempts = failedLoginAttempts,
            AccountLockedUntil = accountLockedUntil,
            Role = role,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt
        };
        return client;
    }
    
    public override void Validate(IValidationHandler handler)
    {
        new ClientValidator(this, handler).Validate();
    }
    public void VerifyEmail()
    {
        if (EmailVerified.HasValue) return;
        EmailVerified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new ClientEmailVerifiedEvent(this.Id));
    }
    public void AddAddress(Address address, IValidationHandler handler)
    {
        address.Validate(handler);
        if (handler.HasError()) return;
        if (address.IsDefault)
        {
            _addresses.Where(a => a.Type == address.Type && a.IsDefault)
                      .ToList()
                      .ForEach(a => a.SetDefault(false));
        }
        _addresses.Add(address);
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddSavedCard(string lastFour, CardBrand brand, string gatewayToken, DateOnly expiryDate, string? nickname)
    {
        bool isDefault = !_savedCards.Any(c => c.DeletedAt == null);
        var newCard = SavedCard.NewCard(Id, lastFour, brand, gatewayToken, expiryDate, isDefault, nickname);
        if (isDefault)
        {
            SetDefaultCard(newCard.Id);
        }
        _savedCards.Add(newCard);
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetDefaultCard(Guid cardId)
    {
        var cardToSet = _savedCards.FirstOrDefault(c => c.Id == cardId && c.DeletedAt == null);
        DomainException.ThrowWhen(cardToSet is null, "Cartão não encontrado ou já foi removido.");
        DomainException.ThrowWhen(cardToSet.IsExpired(), "Não é possível definir um cartão expirado como padrão.");
        _savedCards.Where(c => c.IsDefault).ToList().ForEach(c => c.SetDefault(false));
        cardToSet.SetDefault(true);
        UpdatedAt = DateTime.UtcNow;
    }
    public void RemoveSavedCard(Guid cardId)
    {
        var cardToRemove = _savedCards.FirstOrDefault(c => c.Id == cardId);
        if (cardToRemove != null)
        {
            cardToRemove.SoftDelete();
            UpdatedAt = DateTime.UtcNow;
        }
    }
    public void GiveConsent(ConsentType type, string? termsVersion = null)
    {
        var consent = _consents.FirstOrDefault(c => c.Type == type);
        if (consent != null)
        {
            consent.Grant(termsVersion);
        }
        else
        {
            _consents.Add(Consent.NewConsent(Id, type, true, termsVersion));
        }
        UpdatedAt = DateTime.UtcNow;
    }
    public void RevokeConsent(ConsentType type)
    {
        var consent = _consents.FirstOrDefault(c => c.Type == type);
        if (consent != null)
        {
            consent.Revoke();
            UpdatedAt = DateTime.UtcNow;
        }
    }
    public void PromoteToAdmin()
    {
        if (this.Role == Role.Admin) return;
        this.Role = Role.Admin;
        this.UpdatedAt = DateTime.UtcNow;
    }
    public void HandleFailedLoginAttempt(int maxAttempts, TimeSpan lockoutDuration)
    {
        if (IsLocked) return;
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            AccountLockedUntil = DateTime.UtcNow.Add(lockoutDuration);
        }
        UpdatedAt = DateTime.UtcNow;
    }
    public void ResetLoginAttempts()
    {
        if (FailedLoginAttempts == 0 && !AccountLockedUntil.HasValue) return;
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
