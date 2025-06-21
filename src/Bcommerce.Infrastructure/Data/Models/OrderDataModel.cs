namespace Bcommerce.Infrastructure.Data.Models;

public class OrderDataModel
{
    public Guid order_id { get; set; }
    public string reference_code { get; set; }
    public Guid client_id { get; set; }
    public Guid? coupon_id { get; set; }
    public string status { get; set; } // Mapeado do enum order_status_enum
    public decimal items_total_amount { get; set; }
    public decimal discount_amount { get; set; }
    public decimal shipping_amount { get; set; }
    public decimal grand_total_amount { get; set; } // Coluna gerada
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; }
}