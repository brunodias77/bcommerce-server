namespace Bcommerce.Infrastructure.Data.Models;

public class SavedCardDataModel
{
    public Guid saved_card_id { get; set; }
    public Guid client_id { get; set; }
    public string? nickname { get; set; }
    public string last_four_digits { get; set; }
    public string brand { get; set; } // Mapeado do enum card_brand_enum
    public string gateway_token { get; set; }
    public DateTime expiry_date { get; set; } // O Dapper pode mapear DATE para DateTime
    public bool is_default { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; }
}