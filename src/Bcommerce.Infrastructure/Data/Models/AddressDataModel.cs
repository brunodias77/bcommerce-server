namespace Bcommerce.Infrastructure.Data.Models;

public class AddressDataModel
{
    public Guid address_id { get; set; }
    public Guid client_id { get; set; }
    public string type { get; set; } // O enum do PG será lido como string
    public string postal_code { get; set; }
    public string street { get; set; }
    public string number { get; set; }
    public string? complement { get; set; }
    public string neighborhood { get; set; }
    public string city { get; set; }
    public string state_code { get; set; }
    public string country_code { get; set; }
    public bool is_default { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
}