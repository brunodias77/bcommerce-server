namespace Bcommerce.Infrastructure.Data.Models;

public class ProductDataModel
{
    public Guid product_id { get; set; }
    public string base_sku { get; set; } // ADICIONADO
    public string name { get; set; }
    public string slug { get; set; }
    public string? description { get; set; }
    public decimal base_price { get; set; }
    public decimal? sale_price { get; set; } // ADICIONADO
    public DateTime? sale_price_start_date { get; set; } // ADICIONADO
    public DateTime? sale_price_end_date { get; set; } // ADICIONADO
    public int stock_quantity { get; set; }
    public bool is_active { get; set; }
    public decimal? weight_kg { get; set; } // ADICIONADO
    public int? height_cm { get; set; } // ADICIONADO
    public int? width_cm { get; set; } // ADICIONADO
    public int? depth_cm { get; set; } // ADICIONADO
    public Guid category_id { get; set; }
    public Guid? brand_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; } // ADICIONADO
}
