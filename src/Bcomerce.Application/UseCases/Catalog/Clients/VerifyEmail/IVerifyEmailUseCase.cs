using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;

// O input é o token (string), o output de sucesso é um booleano simples, 
// e o de falha é a nossa notificação de erros.
public interface IVerifyEmailUseCase : IUseCase<string, bool, Notification>
{
}