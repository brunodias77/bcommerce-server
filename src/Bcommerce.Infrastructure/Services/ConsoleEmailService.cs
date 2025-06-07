using Bcommerce.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Infrastructure.Services;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string recipientEmail, string recipientName, string verificationLink, CancellationToken cancellationToken)
    {
        _logger.LogWarning("--- SIMULANDO ENVIO DE E-MAIL ---");
        _logger.LogInformation("Para: {Email}", recipientEmail);
        _logger.LogInformation("Nome: {Name}", recipientName);
        _logger.LogInformation("Link: {Link}", verificationLink);
        _logger.LogWarning("----------------------------------");
        
        return Task.CompletedTask;
    }
}