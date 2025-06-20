namespace Bcomerce.Application.UseCases.Clients.Create;

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