using Bcommerce.Domain.Customers.Clients;

namespace Bcommerce.Domain.Services;

/// <summary>
/// Define o contrato para um serviço que gera tokens de autenticação.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um token e sua respectiva data de expiração com base nos dados de um cliente.
    /// </summary>
    /// <param name="client">A entidade do cliente.</param>
    /// <returns>Uma tupla contendo a string do token de acesso (AccessToken) e a data de expiração (ExpiresAt).</returns>
    AuthResult GenerateTokens(Client client); // Nome e retorno atualizados
}

// Um record para encapsular o resultado da autenticação
public record AuthResult(string AccessToken, DateTime ExpiresAt, string RefreshToken);

