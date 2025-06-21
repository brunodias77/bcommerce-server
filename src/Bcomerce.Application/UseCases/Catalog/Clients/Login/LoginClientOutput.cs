namespace Bcomerce.Application.UseCases.Catalog.Clients.Login;

public record LoginClientOutput(string AccessToken, DateTime ExpiresAt, string RefreshToken);
