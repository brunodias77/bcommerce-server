using System.Security.Claims;
using Bcommerce.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace Bcommerce.Infrastructure.Services;

public class LoggedUser : ILoggedUser
{
    public LoggedUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid GetClientId()
    {
        // "sub" é o nome padrão do claim para o "Subject" (ID do usuário) no padrão JWT
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                       _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
        
        if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out var clientId))
        {
            // Isso não deveria acontecer em um endpoint protegido
            throw new ApplicationException("Não foi possível identificar o usuário logado.");
        }
        
        return clientId;    }
}