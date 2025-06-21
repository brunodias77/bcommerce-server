using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.Domain.Customers.Clients.Entities; // <-- CORREÇÃO: using adicionado

namespace Bcomerce.Application.UseCases.Catalog.Clients.RefreshToken;

public class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RefreshTokenUseCase(
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<Result<LoginClientOutput, Notification>> Execute(RefreshTokenInput input)
    {
        var notification = Notification.Create();
        if (string.IsNullOrWhiteSpace(input.RefreshToken))
        {
            notification.Append(new Error("Refresh Token é obrigatório."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }

        await _uow.Begin();
        try
        {
            var oldToken = await _refreshTokenRepository.GetByTokenValueAsync(input.RefreshToken, CancellationToken.None);

            // Valida se o token existe e está ativo (não expirado e não revogado)
            if (oldToken is null || !oldToken.IsActive)
            {
                notification.Append(new Error("Sessão inválida. Por favor, realize o login novamente."));
                await _uow.Rollback();
                return Result<LoginClientOutput, Notification>.Fail(notification);
            }

            // Revoga o token antigo para que não possa ser reutilizado
            oldToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(oldToken, CancellationToken.None);

            var client = await _clientRepository.Get(oldToken.ClientId, CancellationToken.None);
            if (client is null)
            {
                notification.Append(new Error("Usuário não encontrado."));
                await _uow.Rollback();
                return Result<LoginClientOutput, Notification>.Fail(notification);
            }

            // Gera um novo par de Access Token e Refresh Token
            var authResult = _tokenService.GenerateTokens(client);

            // Cria e salva o novo Refresh Token no banco
            var newRefreshToken = Bcommerce.Domain.Customers.Clients.Entities.RefreshToken.NewToken(
                client.Id,
                authResult.RefreshToken,
                TimeSpan.FromDays(7) // Define a validade do novo token
            );
            await _refreshTokenRepository.AddAsync(newRefreshToken, CancellationToken.None);

            await _uow.Commit();

            var output = new LoginClientOutput(authResult.AccessToken, authResult.ExpiresAt, authResult.RefreshToken);
            return Result<LoginClientOutput, Notification>.Ok(output);
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro ao renovar a sessão."));
            return Result<LoginClientOutput, Notification>.Fail(notification);
        }
    }
}