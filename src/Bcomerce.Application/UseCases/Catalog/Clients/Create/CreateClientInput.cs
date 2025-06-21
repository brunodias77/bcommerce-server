namespace Bcomerce.Application.UseCases.Catalog.Clients.Create;

public record CreateClientInput(        
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber, 
    string Password,    
    bool NewsletterOptIn
    )
{
    
}