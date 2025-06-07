using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Clients.Events;
using Bcommerce.Domain.Clients.Validators;
using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Clients;

public class Client : AggregateRoot
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateTime? EmailVerified { get; private set; }
    public string PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public string? Cpf { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public bool NewsletterOptIn { get; private set; }
    public ClientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Construtor privado para ORM e métodos de fábrica
    private Client() : base() { }
    
    // Método de fábrica público para criar um novo cliente
    public static Client NewClient(
            string fistName, 
            string lastName,
            string email,
            string phoneNumber,
            string passwordHash,
            string? cpf,
            DateOnly? dateOfBirth,
            bool newNewsletterOptIn,
            IValidationHandler validationHandler
        )
    {
        var client = new Client
        {
            FirstName = fistName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Cpf = cpf,
            DateOfBirth = dateOfBirth,
            NewsletterOptIn = newNewsletterOptIn,
            Status = ClientStatus.Ativo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailVerified = null,
            DeletedAt = null
        };
        
        client.Validate(validationHandler);

       if (!validationHandler.HasError())
       {
           client.RaiseEvent(new ClientCreatedEvent(client.Id, client.Email, client.FirstName));
       }
        return client;
    }
    
    // Método de fábrica estático para hidratar/reconstruir um cliente a partir da persistência
    public static Client With(
        Guid id,
        string firstName,
        string lastName,
        string email,
        DateTime? emailVerifiedAt,
        string phone,
        string passwordHash,
        string? cpf,
        DateOnly? dateOfBirth,
        bool newsletterOptIn,
        ClientStatus status,
        DateTime createdAt,
        DateTime updatedAt,
        DateTime? deletedAt)
    {
        var client = new Client
        {
            // Propriedades atribuídas diretamente
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            EmailVerified = emailVerifiedAt,
            PhoneNumber = phone,
            PasswordHash = passwordHash,
            Cpf = cpf,
            DateOfBirth = dateOfBirth,
            NewsletterOptIn = newsletterOptIn,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt
        };
        client.Id = id;
        return client;
    }
    
    public override void Validate(IValidationHandler handler)
    {
        new ClientValidator(this, handler).Validate();
    }
    
    // NOVO: Método para executar a ação de verificação
    public void VerifyEmail()
    {
        // Se já foi verificado, não faz nada
        if (EmailVerified.HasValue) return;

        EmailVerified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        // Opcional: Você poderia disparar um evento "EmailVerifiedEvent" aqui também.
    }
}









// Arquivo: Bcommerce.Domain/Entities/Clients/Client.cs
// namespace Bcommerce.Domain.Entities.Clients
// {
//     using Bcommerce.Domain.Abstractions;
//     using Bcommerce.Domain.Validations;
//     using Bcommerce.Domain.Entities.Clients.Enums;
//     using Bcommerce.Domain.Entities.Clients.Validators;
//     using Bcommerce.Domain.DomainEvents; // Para ClientCreatedDomainEvent
//     using System;
//
//     public class Client : AggregateRoot
//     {
//         public string FirstName { get; private set; }
//         public string LastName { get; private set; }
//         public string Email { get; private set; }
//         public DateTime? EmailVerifiedAt { get; private set; }
//         public string Phone { get; private set; }
//         public string PasswordHash { get; private set; }
//         public string? Cpf { get; private set; }
//         public DateOnly? DateOfBirth { get; private set; }
//         public bool NewsletterOptIn { get; private set; }
//         public ClientStatus Status { get; private set; }
//         public DateTime CreatedAt { get; private set; }
//         public DateTime UpdatedAt { get; private set; }
//         public DateTime? DeletedAt { get; private set; }
//
//         // Construtor privado para ORM e métodos de fábrica
//         private Client() : base() { }
//
//         // Método de fábrica público para criar um novo cliente
//         public static Client CreateNew(
//             string firstName,
//             string lastName,
//             string email,
//             string phone,
//             string passwordHash, // A senha deve ser previamente hasheada antes de chegar à camada de domínio
//             string? cpf,
//             DateOnly? dateOfBirth,
//             bool newsletterOptIn,
//             IValidationHandler validationHandler)
//         {
//             var client = new Client
//             {
//                 // O Id é gerado pelo construtor da entidade base
//                 FirstName = firstName,
//                 LastName = lastName,
//                 Email = email,
//                 Phone = phone,
//                 PasswordHash = passwordHash,
//                 Cpf = cpf,
//                 DateOfBirth = dateOfBirth,
//                 NewsletterOptIn = newsletterOptIn,
//                 Status = ClientStatus.Ativo, // Status padrão para novos clientes
//                 CreatedAt = DateTime.UtcNow,
//                 UpdatedAt = DateTime.UtcNow, // Inicialmente UpdatedAt é igual ao CreatedAt
//                 EmailVerifiedAt = null,
//                 DeletedAt = null
//             };
//
//             client.Validate(validationHandler);
//
//             if (!validationHandler.HasError())
//             {
//                 client.RaiseEvent(new ClientCreatedDomainEvent(client.Id, client.Email));
//             }
//             
//             return client;
//         }
//
//         // Método de fábrica estático para hidratar/reconstruir um cliente a partir da persistência
//         public static Client Hydrate(
//             Guid id,
//             string firstName,
//             string lastName,
//             string email,
//             DateTime? emailVerifiedAt,
//             string phone,
//             string passwordHash,
//             string? cpf,
//             DateOnly? dateOfBirth,
//             bool newsletterOptIn,
//             ClientStatus status,
//             DateTime createdAt,
//             DateTime updatedAt,
//             DateTime? deletedAt)
//         {
//             var client = new Client
//             {
//                 // Propriedades atribuídas diretamente
//                 FirstName = firstName,
//                 LastName = lastName,
//                 Email = email,
//                 EmailVerifiedAt = emailVerifiedAt,
//                 Phone = phone,
//                 PasswordHash = passwordHash,
//                 Cpf = cpf,
//                 DateOfBirth = dateOfBirth,
//                 NewsletterOptIn = newsletterOptIn,
//                 Status = status,
//                 CreatedAt = createdAt,
//                 UpdatedAt = updatedAt,
//                 DeletedAt = deletedAt
//             };
//             // Sobrescreve o Id gerado pelo construtor sem parâmetros
//             // Isso é possível pois Entity.Id tem um setter protegido
//             client.Id = id;
//             return client;
//         }
//
//         public void UpdateProfile(string firstName, string lastName, string phone, DateOnly? dateOfBirth, IValidationHandler handler)
//         {
//             FirstName = firstName;
//             LastName = lastName;
//             Phone = phone;
//             DateOfBirth = dateOfBirth;
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler);
//             // Opcionalmente: RaiseEvent(new ClientProfileUpdatedEvent(Id));
//         }
//
//         public void ChangeEmail(string newEmail, IValidationHandler handler)
//         {
//             Email = newEmail;
//             EmailVerifiedAt = null; // O e-mail precisa ser verificado novamente
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler); // Valida a mudança, especialmente o novo formato de e-mail
//             // Opcionalmente: RaiseEvent(new ClientEmailChangedEvent(Id, newEmail));
//         }
//
//         public void VerifyEmail()
//         {
//             if (EmailVerifiedAt.HasValue) return; // Já está verificado
//
//             EmailVerifiedAt = DateTime.UtcNow;
//             UpdatedAt = DateTime.UtcNow;
//             // Opcionalmente: RaiseEvent(new ClientEmailVerifiedEvent(Id));
//         }
//
//         public void ChangePassword(string newPasswordHash, IValidationHandler handler)
//         {
//             PasswordHash = newPasswordHash; // Assume-se que o hash já foi validado quanto à força na camada de aplicação
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler); // Valida o hash (ex: não vazio)
//             // Opcionalmente: RaiseEvent(new ClientPasswordChangedEvent(Id));
//         }
//
//         public void UpdateCpf(string? newCpf, IValidationHandler handler)
//         {
//             Cpf = newCpf;
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler); // Valida o novo CPF
//             // Opcionalmente: RaiseEvent(new ClientCpfUpdatedEvent(Id));
//         }
//
//         public void SetNewsletterSubscription(bool optIn)
//         {
//             if (NewsletterOptIn == optIn) return;
//
//             NewsletterOptIn = optIn;
//             UpdatedAt = DateTime.UtcNow;
//             // Normalmente não há validação específica, mas poderia chamar Validate se houvesse regras complexas
//             // Opcionalmente: RaiseEvent(new ClientNewsletterSubscriptionChangedEvent(Id, optIn));
//         }
//
//         public void ChangeStatus(ClientStatus newStatus, IValidationHandler handler)
//         {
//             if (Status == newStatus) return;
//
//             Status = newStatus;
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler); // Caso existam regras para transição de status
//             // Opcionalmente: RaiseEvent(new ClientStatusChangedEvent(Id, newStatus));
//         }
//
//         public void SoftDelete(IValidationHandler handler)
//         {
//             if (DeletedAt.HasValue) return; // Já está deletado
//
//             DeletedAt = DateTime.UtcNow;
//             Status = ClientStatus.Inativo; // Ou um status específico "Deletado"
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler); // Normalmente uma validação mínima aqui
//             // Opcionalmente: RaiseEvent(new ClientSoftDeletedEvent(Id));
//         }
//
//         public void Restore(IValidationHandler handler)
//         {
//             if (!DeletedAt.HasValue) return; // Não está deletado
//
//             DeletedAt = null;
//             Status = ClientStatus.Ativo; // Ou restaurar para um status anterior, se rastreado
//             UpdatedAt = DateTime.UtcNow;
//             Validate(handler);
//             // Opcionalmente: RaiseEvent(new ClientRestoredEvent(Id));
//         }
//
//         public override void Validate(IValidationHandler handler)
//         {
//             new ClientValidator(this, handler).Validate();
//         }
//     }
// }
