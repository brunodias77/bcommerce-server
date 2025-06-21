namespace Bcommerce.Infrastructure.Data.Models;

public class ProductImageDataModel
{
    public Guid product_image_id { get; set; }
    public Guid product_id { get; set; }
    public string image_url { get; set; }
    public string? alt_text { get; set; }
    public bool is_cover { get; set; }
    public int sort_order { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; }
}