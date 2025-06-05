namespace Bcomerce.Application.UseCases.Clients.Create;

public record CreateClientInput(        string FirstName,
    string LastName,
    string Email,
    string PhoneNumber, // Corresponde à propriedade Client.PhoneNumber
    string Password,    // Senha em texto plano
    string? Cpf,
    DateOnly? DateOfBirth,
    bool NewsletterOptIn)
{
    
}