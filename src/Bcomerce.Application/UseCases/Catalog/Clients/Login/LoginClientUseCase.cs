using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;

public class LoginClientUseCase : ILoginClientUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly IPasswordEncripter _passwordEncrypter;
    private readonly ITokenService _tokenService;

    public LoginClientUseCase(IClientRepository clientRepository, IPasswordEncripter passwordEncrypter, ITokenService tokenService)
    {
        _clientRepository = clientRepository;
        _passwordEncrypter = passwordEncrypter;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginClientOutput, Notification>> Execute(LoginClientInput input)
    {
        var notification = Notification.Create();
        
        var client = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
        var invalidCredentialsError = new Error("E-mail ou senha inválidos.");
        
        if (client is null)
        {
            notification.Append(invalidCredentialsError);
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        if (!client.EmailVerified.HasValue)
        {
            notification.Append(new Error("Seu e-mail ainda não foi verificado. Por favor, verifique sua caixa de entrada."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        var isPasswordValid = _passwordEncrypter.Verify(input.Password, client.PasswordHash);
        
        if (!isPasswordValid)
        {
            notification.Append(invalidCredentialsError);
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // CORREÇÃO: A lógica de expiração foi removida daqui e agora vem diretamente do ITokenService.
        var (accessToken, expiresAt) = _tokenService.GenerateToken(client);
        var output = new LoginClientOutput(accessToken, expiresAt);
        
        return Result<LoginClientOutput, Notification>.Ok(output);
    }
}