namespace Bcommerce.Infrastructure.Data.Models;

public class ProductVariantDataModel
{
    public Guid product_variant_id { get; set; }
    public Guid product_id { get; set; }
    public string sku { get; set; }
    public Guid? color_id { get; set; }
    public Guid? size_id { get; set; }
    public int stock_quantity { get; set; }
    public decimal additional_price { get; set; }
    public string? image_url { get; set; } // ADICIONADO
    public bool is_active { get; set; }
}
