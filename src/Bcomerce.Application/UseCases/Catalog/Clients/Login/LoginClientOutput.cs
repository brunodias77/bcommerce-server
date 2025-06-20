namespace Bcomerce.Application.UseCases.Clients.Login;

public record LoginClientOutput(string AccessToken, DateTime ExpiresAt);
