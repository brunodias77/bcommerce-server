namespace Bcommerce.Infrastructure.Data.Models;

public class PaymentDataModel
{
    public Guid payment_id { get; set; }
    public Guid order_id { get; set; }
    public string method { get; set; } // Mapeado do enum payment_method_enum
    public string status { get; set; } // Mapeado do enum payment_status_enum
    public decimal amount { get; set; }
    public string? transaction_id { get; set; }
    public string? method_details { get; set; } // Mapeado do JSONB como string
    public DateTime? processed_at { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public int version { get; set; }
}