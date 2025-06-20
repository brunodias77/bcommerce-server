using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.Login;

public class LoginClientUseCase : ILoginClientUseCase
{
    public LoginClientUseCase(IClientRepository clientRepository, IPasswordEncripter passwordEncrypter, ITokenService tokenService)
    {
        _clientRepository = clientRepository;
        _passwordEncrypter = passwordEncrypter;
        _tokenService = tokenService;
    }

    private readonly IClientRepository _clientRepository;
    private readonly IPasswordEncripter _passwordEncrypter;
    private readonly ITokenService _tokenService;
    public async Task<Result<LoginClientOutput, Notification>> Execute(LoginClientInput input)
    {
        var notification = Notification.Create();
        
        var client = await _clientRepository.GetByEmail(input.Email, CancellationToken.None);
        // Mensagem de erro genérica para não informar se o e-mail existe ou se a senha está errada
        var invalidCredentialsError = new Error("E-mail ou senha inválidos.");
        
        if (client is null)
        {
            notification.Append(invalidCredentialsError);
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
        
        // REGRA DE NEGÓCIO: Não permitir login se o e-mail não foi verificado
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
        
        var token = _tokenService.GenerateToken(client);
        var expiresAt = DateTime.UtcNow.AddMinutes(60); 
        var output = new LoginClientOutput(token, expiresAt);
        
        return Result<LoginClientOutput, Notification>.Ok(output);
    }
}