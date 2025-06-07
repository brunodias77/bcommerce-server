namespace Bcommerce.Domain.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string recipientEmail, string recipientName, string verificationLink, CancellationToken cancellationToken);

}