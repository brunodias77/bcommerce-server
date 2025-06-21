namespace Bcommerce.Infrastructure.Data.Models;

public class OrderItemDataModel
{
    public Guid order_item_id { get; set; }
    public Guid order_id { get; set; }
    public Guid? product_variant_id { get; set; }
    public string item_sku { get; set; }
    public string item_name { get; set; }
    public int quantity { get; set; }
    public decimal unit_price { get; set; }
    public decimal line_item_total_amount { get; set; } // Coluna gerada
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public int version { get; set; }
}