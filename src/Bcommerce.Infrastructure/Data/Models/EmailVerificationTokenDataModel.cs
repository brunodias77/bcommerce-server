namespace Bcommerce.Infrastructure.Data.Models;

// Objeto simples para mapeamento direto da tabela 'email_verification_tokens'.
public class EmailVerificationTokenDataModel
{
    public Guid token_id { get; set; }
    public Guid client_id { get; set; }
    public string token_hash { get; set; }
    public DateTime expires_at { get; set; }
    public DateTime created_at { get; set; }
}