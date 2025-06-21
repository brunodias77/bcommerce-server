using Bcommerce.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Bcommerce.Domain.Customers.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bcommerce.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    public AuthResult GenerateTokens(Client client)
    {
        // 1. Geração do Access Token (lógica que você já tem)
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

        var expires = DateTime.UtcNow.AddMinutes(15); // << Reduzir tempo de vida do Access Token

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
        
        // 2. Geração do Refresh Token
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        // 3. Retornar ambos os tokens
        return new AuthResult(accessToken, expires, refreshToken);
    }
}