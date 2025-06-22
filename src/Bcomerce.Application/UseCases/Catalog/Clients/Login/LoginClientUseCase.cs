using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;

public class LoginClientUseCase : ILoginClientUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly IPasswordEncripter _passwordEncrypter;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;

    public LoginClientUseCase(
        IClientRepository clientRepository, IPasswordEncripter passwordEncrypter, ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository, IUnitOfWork uow, IConfiguration configuration)
    {
        _clientRepository = clientRepository;
        _passwordEncrypter = passwordEncrypter;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _uow = uow;
        _configuration = configuration;
    }

    public async Task<Result<LoginClientOutput, Notification>> Execute(LoginClientInput input)
    {
        var notification = Notification.Create();
        
        var client = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
        
        // Validação primária: o cliente existe?
        if (client is null)
        {
            notification.Append(new Error("E-mail ou senha inválidos."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // Validação secundária: a conta está bloqueada?
        if (client.IsLocked)
        {
            var remainingTime = client.AccountLockedUntil!.Value - DateTime.UtcNow;
            notification.Append(new Error($"Sua conta está temporariamente bloqueada. Tente novamente em {Math.Ceiling(remainingTime.TotalMinutes)} minutos."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // Validação terciária: o e-mail foi verificado?
        if (!client.EmailVerified.HasValue)
        {
            notification.Append(new Error("Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        var isPasswordValid = _passwordEncrypter.Verify(input.Password, client.PasswordHash);
        
        // Se a senha for inválida, trata a falha e encerra.
        if (!isPasswordValid)
        {
            var maxAttempts = _configuration.GetValue<int>("Settings:AccountLockout:MaxFailedAccessAttempts", 5);
            var lockoutMinutes = _configuration.GetValue<int>("Settings:AccountLockout:DefaultLockoutMinutes", 15);

            client.HandleFailedLoginAttempt(maxAttempts, TimeSpan.FromMinutes(lockoutMinutes));

            await _uow.Begin();
            await _clientRepository.Update(client, CancellationToken.None);
            await _uow.Commit();

            notification.Append(new Error("E-mail ou senha inválidos."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }

        // Se chegou até aqui, o login é bem-sucedido. Zera as tentativas e gera os tokens.
        await _uow.Begin();
        try
        {
            client.ResetLoginAttempts();
            await _clientRepository.Update(client, CancellationToken.None);

            var authResult = _tokenService.GenerateTokens(client);
            var refreshTokenEntity = Bcommerce.Domain.Customers.Clients.Entities.RefreshToken.NewToken(client.Id, authResult.RefreshToken, TimeSpan.FromDays(7));
            await _refreshTokenRepository.AddAsync(refreshTokenEntity, CancellationToken.None);
            
            await _uow.Commit();

            var output = new LoginClientOutput(authResult.AccessToken, authResult.ExpiresAt, authResult.RefreshToken);
            return Result<LoginClientOutput, Notification>.Ok(output);
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Não foi possível iniciar sua sessão. Tente novamente."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
    }
}
