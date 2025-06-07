using System.Security.Cryptography;
using System.Text;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients.Events;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcommerce.Application.Clients.Events;

public class ClientCreatedEventHandler : IDomainEventHandler<ClientCreatedEvent>
{
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _uow;

    public ClientCreatedEventHandler(IEmailVerificationTokenRepository tokenRepository, IEmailService emailService, IUnitOfWork uow)
    {
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _uow = uow;
    }

    public async Task HandleAsync(ClientCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // 1. Gerar um token seguro
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_'); // URL-safe token

        // 2. Gerar o Hash do token para armazenar no banco
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // 3. Criar a entidade do token
        var verificationToken = new EmailVerificationToken
        {
            TokenId = Guid.NewGuid(),
            ClientId = domainEvent.ClientId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // Token expira em 24 horas
            CreatedAt = DateTime.UtcNow
        };
        
        await _uow.Begin();
        try
        {
            await _tokenRepository.AddAsync(verificationToken, cancellationToken);
            await _uow.Commit();
        }
        catch
        {
            await _uow.Rollback();
            throw; // Re-lança a exceção para ser logada mais acima
        }

        // 4. Montar o link de verificação
        var verificationLink = $"https://sua-url-de-api.com/api/clients/verify-email?token={token}";

        // 5. Enviar o email
        await _emailService.SendVerificationEmailAsync(
            domainEvent.Email,
            domainEvent.FirstName,
            verificationLink,
            cancellationToken
        );
    }
}