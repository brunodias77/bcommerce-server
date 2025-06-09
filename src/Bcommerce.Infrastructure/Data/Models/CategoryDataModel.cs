namespace Bcommerce.Infrastructure.Data.Models;

public class CategoryDataModel
{
    public Guid category_id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string? description { get; set; }
    public Guid? parent_category_id { get; set; }
    public bool is_active { get; set; }
    public int sort_order { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
}