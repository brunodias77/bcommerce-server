namespace Bcommerce.Infrastructure.Data.Models;

public class ReviewDataModel
{
    public Guid review_id { get; set; }
    public Guid? client_id { get; set; }
    public Guid product_id { get; set; }
    public Guid? order_id { get; set; }
    public short rating { get; set; }
    public string? comment { get; set; }
    public bool is_approved { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public int version { get; set; }
}