using Bcommerce.Domain.Clients;

namespace Bcommerce.Domain.Services;

public interface ITokenService
{
    string GenerateToken(Client client);
}