namespace Bcommerce.Infrastructure.Data.Models;

public class AddressDataModel
{
    public Guid address_id { get; set; }
    public Guid client_id { get; set; }
    public string type { get; set; }
    public string postal_code { get; set; }
    public string street { get; set; }
    public string street_number { get; set; } // RENOMEADO
    public string? complement { get; set; }
    public string neighborhood { get; set; }
    public string city { get; set; }
    public string state_code { get; set; }
    public string country_code { get; set; } // ADICIONADO
    public bool is_default { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
}