using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Microsoft.AspNetCore.Http;

namespace Bcomerce.Application.UseCases.Catalog.Clients.Logout;

public class LogoutUseCase : ILogoutUseCase
{
    public LogoutUseCase(IHttpContextAccessor httpContextAccessor, IRevokedTokenRepository revokedTokenRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _revokedTokenRepository = revokedTokenRepository;
    }

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRevokedTokenRepository _revokedTokenRepository;
    public async Task<Result<bool, Notification>> Execute(object input)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Result<bool, Notification>.Ok(true); // Se não está logado, logout é bem-sucedido.
        }
        // Extrai as claims 'jti' e 'exp' do token atual.
        var jtiClaim = user.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = user.FindFirstValue(JwtRegisteredClaimNames.Exp);
        var subClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(jtiClaim, out var jti) || 
            !long.TryParse(expClaim, out var exp) ||
            !Guid.TryParse(subClaim, out var clientId))
        {
            // Não deve acontecer com um token válido, mas é uma proteção.
            var notification = Notification.Create().Append(new Error("Token inválido para logout."));
            return Result<bool, Notification>.Fail(notification);
        }

        var expiresAt = DateTime.UnixEpoch.AddSeconds(exp);
        
        await _revokedTokenRepository.AddAsync(jti, clientId, expiresAt, CancellationToken.None);

        return Result<bool, Notification>.Ok(true);    }
}