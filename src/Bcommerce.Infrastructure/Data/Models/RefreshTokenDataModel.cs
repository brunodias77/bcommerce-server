namespace Bcommerce.Infrastructure.Data.Models;

public class RefreshTokenDataModel
{
    public Guid token_id { get; set; }
    public Guid client_id { get; set; }
    public string token_value { get; set; }
    public DateTime expires_at { get; set; }
    public DateTime created_at { get; set; }
    public DateTime? revoked_at { get; set; }
}