namespace Bcommerce.Infrastructure.Data.Models;

public class CartDataModel
{
    public Guid cart_id { get; set; }
    public Guid? client_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? expires_at { get; set; }
}