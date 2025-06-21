using System.Security.Cryptography;
using System.Text;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Events;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;


namespace Bcommerce.Application.Events.Clients;

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
        // 1. Gera um token seguro e URL-friendly para o usuário
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // 2. Gera o Hash do token que será armazenado no banco
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // 3. CORREÇÃO: Utiliza o método de fábrica da entidade para criar o token.
        //    Isso centraliza as regras de negócio e a criação do Id na própria entidade.
        var verificationToken = EmailVerificationToken.NewToken(
            domainEvent.ClientId,
            tokenHash,
            TimeSpan.FromHours(24) // Define a validade do token
        );
        
        await _uow.Begin();
        try
        {
            await _tokenRepository.AddAsync(verificationToken, cancellationToken);
            await _uow.Commit();
        }
        catch
        {
            await _uow.Rollback();
            throw; // Re-lança a exceção para ser registrada pelos logs da aplicação
        }

        // 4. Monta o link de verificação que será enviado no e-mail
        var verificationLink = $"http://localhost:3000/auth/verify-email?token={token}";

        // 5. Dispara o serviço de envio de e-mail
        await _emailService.SendVerificationEmailAsync(
            domainEvent.Email,
            domainEvent.FirstName,
            verificationLink,
            cancellationToken
        );
    }
}