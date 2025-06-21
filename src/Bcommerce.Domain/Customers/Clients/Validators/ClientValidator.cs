using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Validators;

public class ClientValidator : Validator
{
    private readonly Client _client;
    private const int NameMaxLength = 100;

    public ClientValidator(Client client, IValidationHandler handler) : base(handler)
    {
        _client = client;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_client.FirstName))
            ValidationHandler.Append(new Error("'FirstName' é obrigatório."));

        if (_client.FirstName?.Length > NameMaxLength)
            ValidationHandler.Append(new Error($"'FirstName' não pode exceder {NameMaxLength} caracteres."));

        if (string.IsNullOrWhiteSpace(_client.LastName))
            ValidationHandler.Append(new Error("'LastName' é obrigatório."));

        if (string.IsNullOrWhiteSpace(_client.PasswordHash))
            ValidationHandler.Append(new Error("A senha não pode estar vazia."));
            
        // A validação do Email e do Cpf já é feita dentro dos seus respectivos Value Objects.
        // O Notification handler captura os erros se eles ocorrerem durante a criação desses objetos.
    }
}