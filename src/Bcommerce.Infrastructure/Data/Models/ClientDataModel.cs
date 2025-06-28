namespace Bcommerce.Infrastructure.Data.Models;

public class ClientDataModel
{
    public Guid client_id { get; set; }
    public string first_name { get; set; } = string.Empty;
    public string last_name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public DateTime? email_verified_at { get; set; }
    public string phone { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public string? cpf { get; set; }
    public DateTime? date_of_birth { get; set; } // Postgres DATE maps to DateTime; Npgsql 6+ handles DateOnly
    public bool newsletter_opt_in { get; set; }
    public string status { get; set; } = string.Empty; // Ler como string para conversão manual para enum
    public string role { get; set; } = string.Empty;
    public int failed_login_attempts { get; set; }
    public DateTime? account_locked_until { get; set; }
    
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? deleted_at { get; set; }
}