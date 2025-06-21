using System.Security.Cryptography;
using System.Text;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;

public class VerifyEmailUseCase : IVerifyEmailUseCase
{
    public VerifyEmailUseCase(IClientRepository clientRepository, IEmailVerificationTokenRepository tokenRepository, IUnitOfWork uow, ILogger<VerifyEmailUseCase> logger)
    {
        _clientRepository = clientRepository;
        _tokenRepository = tokenRepository;
        _uow = uow;
        _logger = logger;
    }

    private readonly IClientRepository _clientRepository;
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<VerifyEmailUseCase> _logger;
    public async Task<Result<bool, Notification>> Execute(string token)
    {
        var notification = Notification.Create();
        if (string.IsNullOrWhiteSpace(token))
        {
            notification.Append(new Error("Token de verificação inválido."));
            return Result<bool, Notification>.Fail(notification);
        }
        
        // É crucial usar o mesmo algoritmo de hash usado ao criar o token
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        await _uow.Begin();
        
        try
        {
            var verificationToken = await _tokenRepository.GetByTokenHashAsync(tokenHash, CancellationToken.None);

            if (verificationToken is null || verificationToken.ExpiresAt < DateTime.UtcNow)
            {
                if (verificationToken is not null) // Se o token existe mas está expirado, removemos
                {
                    await _tokenRepository.DeleteAsync(verificationToken, CancellationToken.None);
                    await _uow.Commit();
                }
                notification.Append(new Error("Token de verificação inválido ou expirado."));
                return Result<bool, Notification>.Fail(notification);
            }
            
            var client = await _clientRepository.Get(verificationToken.ClientId, CancellationToken.None);
            if (client is null)
            {
                notification.Append(new Error("Usuário não encontrado."));
                return Result<bool, Notification>.Fail(notification);
            }

            // Executa a lógica de negócio na entidade
            client.VerifyEmail();

            await _clientRepository.Update(client, CancellationToken.None);
            await _tokenRepository.DeleteAsync(verificationToken, CancellationToken.None); // Token usado não pode ser usado de novo

            await _uow.Commit();

            return Result<bool, Notification>.Ok(true);
        }
        catch (Exception e)
        {
            if (_uow.HasActiveTransaction) await _uow.Rollback();
            _logger.LogError(e, "Erro inesperado ao verificar o e-mail.");
            notification.Append(new Error("Ocorreu um erro no servidor. Tente novamente mais tarde."));
            return Result<bool, Notification>.Fail(notification);
        }
    }
}