using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories; // Para IUnitOfWork

namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;

public class LoginClientUseCase : ILoginClientUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly IPasswordEncripter _passwordEncrypter;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _uow;

    // Construtor atualizado com as novas injeções de dependência
    public LoginClientUseCase(
        IClientRepository clientRepository,
        IPasswordEncripter passwordEncrypter,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork uow)
    {
        _clientRepository = clientRepository;
        _passwordEncrypter = passwordEncrypter;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _uow = uow;
    }

    public async Task<Result<LoginClientOutput, Notification>> Execute(LoginClientInput input)
    {
        var notification = Notification.Create();
        
        var client = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
        var invalidCredentialsError = new Error("E-mail ou senha inválidos.");
        
        if (client is null || !client.EmailVerified.HasValue)
        {
            var errorMessage = client is null ? "E-mail ou senha inválidos." : "Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada.";
            notification.Append(new Error(errorMessage));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        var isPasswordValid = _passwordEncrypter.Verify(input.Password, client.PasswordHash);
        
        if (!isPasswordValid)
        {
            notification.Append(invalidCredentialsError);
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // Gera o par de tokens (Access + Refresh)
        var authResult = _tokenService.GenerateTokens(client);

        // Cria a entidade para o novo Refresh Token
        var refreshTokenEntity = Bcommerce.Domain.Customers.Clients.Entities.RefreshToken.NewToken(
            client.Id,
            authResult.RefreshToken,
            TimeSpan.FromDays(7) // Define a validade (ex: 7 dias)
        );

        await _uow.Begin();
        try
        {
            // Salva o Refresh Token no banco de dados
            await _refreshTokenRepository.AddAsync(refreshTokenEntity, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Não foi possível iniciar sua sessão. Tente novamente."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // Retorna o resultado completo, incluindo o Refresh Token
        var output = new LoginClientOutput(authResult.AccessToken, authResult.ExpiresAt, authResult.RefreshToken);
        
        return Result<LoginClientOutput, Notification>.Ok(output);
    }
}


// using Bcomerce.Application.Abstractions;
// using Bcomerce.Application.UseCases.Catalog.Clients.Login;
// using Bcommerce.Domain.Customers.Clients.Repositories;
// using Bcommerce.Domain.Security;
// using Bcommerce.Domain.Services;
// using Bcommerce.Domain.Validation;
// using Bcommerce.Domain.Validation.Handlers;
//
// namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;
//
// public class LoginClientUseCase : ILoginClientUseCase
// {
//     private readonly IClientRepository _clientRepository;
//     private readonly IPasswordEncripter _passwordEncrypter;
//     private readonly ITokenService _tokenService;
//
//     public LoginClientUseCase(IClientRepository clientRepository, IPasswordEncripter passwordEncrypter, ITokenService tokenService)
//     {
//         _clientRepository = clientRepository;
//         _passwordEncrypter = passwordEncrypter;
//         _tokenService = tokenService;
//     }
//
//     public async Task<Result<LoginClientOutput, Notification>> Execute(LoginClientInput input)
//     {
//         var notification = Notification.Create();
//         
//         var client = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
//         var invalidCredentialsError = new Error("E-mail ou senha inválidos.");
//         
//         if (client is null)
//         {
//             notification.Append(invalidCredentialsError);
//             return Result<LoginClientOutput, Notification>.Fail(notification);
//         }
//         
//         if (!client.EmailVerified.HasValue)
//         {
//             notification.Append(new Error("Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada."));
//             return Result<LoginClientOutput, Notification>.Fail(notification);
//         }
//         
//         var isPasswordValid = _passwordEncrypter.Verify(input.Password, client.PasswordHash);
//         
//         if (!isPasswordValid)
//         {
//             notification.Append(invalidCredentialsError);
//             return Result<LoginClientOutput, Notification>.Fail(notification);
//         }
//         
//         // CORREÇÃO: A lógica de expiração foi removida daqui e agora vem diretamente do ITokenService.
//         var (accessToken, expiresAt) = _tokenService.GenerateToken(client);
//         var output = new LoginClientOutput(accessToken, expiresAt);
//         
//         return Result<LoginClientOutput, Notification>.Ok(output);
//     }
// }