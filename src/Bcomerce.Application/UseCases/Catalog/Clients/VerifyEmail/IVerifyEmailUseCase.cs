using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.VerifyEmail;

// O input é o token (string), o output de sucesso é um booleano simples, 
// e o de falha é a nossa notificação de erros.
public interface IVerifyEmailUseCase : IUseCase<string, bool, Notification>
{
}