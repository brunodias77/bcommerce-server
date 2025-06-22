using System.IdentityModel.Tokens.Jwt;
using Bcommerce.Domain.Security;

namespace Bcommerce.Api.Middlewares;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IRevokedTokenRepository revokedTokenRepository)
    {
        // Se o usuário está autenticado, o middleware do JWT já validou o token.
        // Agora, fazemos nossa verificação extra.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jtiClaim = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (Guid.TryParse(jtiClaim, out var jti))
            {
                if (await revokedTokenRepository.IsTokenRevokedAsync(jti, context.RequestAborted))
                {
                    // Se o token está na denylist, define a resposta como 401 e encerra o pipeline.
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked.");
                    return;
                }
            }
        }

        // Se o token for válido, continua para o próximo middleware no pipeline.
        await _next(context);
    }
}