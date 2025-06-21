namespace Bcommerce.Infrastructure.Data.Models;

public class CouponDataModel
{
    public Guid coupon_id { get; set; }
    public string code { get; set; }
    public string? description { get; set; }
    public decimal? discount_percentage { get; set; }
    public decimal? discount_amount { get; set; }
    public DateTime valid_from { get; set; }
    public DateTime valid_until { get; set; }
    public int? max_uses { get; set; }
    public int times_used { get; set; }
    public decimal? min_purchase_amount { get; set; }
    public bool is_active { get; set; }
    public string type { get; set; } // Mapeado do enum coupon_type
    public Guid? client_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; }
}