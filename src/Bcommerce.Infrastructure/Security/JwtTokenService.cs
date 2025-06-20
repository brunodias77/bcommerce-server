using Bcommerce.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Services;
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

    public string GenerateToken(Client client)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // A chave secreta é lida do appsettings.json
        var signinKey = _configuration["Settings:JwtSettings:SigninKey"];
        var key = Encoding.ASCII.GetBytes(signinKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()), // Subject (o ID do usuário)
            
            // CORREÇÃO APLICADA AQUI:
            // Acessamos a propriedade .Value do Value Object Email.
            new(JwtRegisteredClaimNames.Email, client.Email.Value), 
            
            new(JwtRegisteredClaimNames.Name, client.FirstName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID (para evitar replay attacks)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Settings:JwtSettings:ExpirationTimeMinutes"])),
            Issuer = _configuration["Settings:JwtSettings:Issuer"],
            Audience = _configuration["Settings:JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}