using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Validations.Handlers;
using Bogus;

namespace Bcommerce.UnitTest.Common;

public class ClientBuilder
{
    private readonly Faker _faker = FakerGenerator.Faker;

    // Propriedades com valores padrão válidos
    private string _firstName;
    private string _lastName;
    private string _email;
    private string _phoneNumber;
    private string _passwordHash = "valid_hashed_password";
    private bool _newsletterOptIn = true;

    private ClientBuilder()
    {
        // Inicializa com dados falsos e válidos
        _firstName = _faker.Person.FirstName;
        _lastName = _faker.Person.LastName;
        _email = _faker.Person.Email;
        _phoneNumber = _faker.Person.Phone;
    }

    // Ponto de entrada para criar o builder
    public static ClientBuilder New()
    {
        return new ClientBuilder();
    }

    // Métodos fluentes para customizar o objeto
    public ClientBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public ClientBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    // Crie outros métodos "With..." para as propriedades que você precisar customizar

    // O método final que constrói a entidade
    public Client Build()
    {
        // Usamos um Notification vazio aqui porque o builder
        // deve sempre gerar uma entidade válida por padrão.
        var notification = Notification.Create();
        
        return Client.NewClient(
            _firstName,
            _lastName,
            _email,
            _phoneNumber,
            _passwordHash,
            null, // cpf
            null, // date of birth
            _newsletterOptIn,
            notification
        );
    }
}