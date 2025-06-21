using Bcommerce.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bcommerce.Domain.Customers.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bcommerce.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gera um token JWT e sua data de expiração.
    /// </summary>
    /// <param name="client">A entidade do cliente para a qual o token será gerado.</param>
    /// <returns>Uma tupla com o token e sua data de expiração.</returns>
    public (string AccessToken, DateTime ExpiresAt) GenerateToken(Client client)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var signinKey = _configuration["Settings:JwtSettings:SigninKey"];
        var key = Encoding.ASCII.GetBytes(signinKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, client.Email.Value), 
            new(JwtRegisteredClaimNames.Name, client.FirstName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Settings:JwtSettings:ExpirationTimeMinutes"]));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _configuration["Settings:JwtSettings:Issuer"],
            Audience = _configuration["Settings:JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // CORREÇÃO: Retorna o token e a data de expiração calculada a partir da configuração.
        return (AccessToken: accessToken, ExpiresAt: expires);
    }
}