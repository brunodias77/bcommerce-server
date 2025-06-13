namespace Bcommerce.Infrastructure.Data.Models;

public class ProductDataModel
{
    public Guid product_id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string? description { get; set; }
    public decimal base_price { get; set; }
    public int stock_quantity { get; set; }
    public bool is_active { get; set; }
    public Guid category_id { get; set; }
    public Guid? brand_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
}