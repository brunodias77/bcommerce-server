namespace Bcommerce.Infrastructure.Data.Models;

public class CartItemDataModel
{
    public Guid cart_item_id { get; set; }
    public Guid cart_id { get; set; }
    public Guid product_variant_id { get; set; }
    public int quantity { get; set; }
    public decimal unit_price { get; set; } // ADICIONADO
    public string currency { get; set; } // ADICIONADO
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
