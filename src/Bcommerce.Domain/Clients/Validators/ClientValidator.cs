using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Clients.Validators;

public class ClientValidator : Validator
{
    private readonly Client _client;

    public ClientValidator(Client client, IValidationHandler handler) : base(handler)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public override void Validate()
    {
        CheckFirstName();
        CheckLastName();
        CheckEmail();
        CheckPhone();
        CheckPasswordHash();
        CheckCpf();
        // DateOfBirth: Business rules like minimum age could be added here.
        // NewsletterOptIn: Boolean, typically no validation needed unless specific rules apply.
        // Status: Enum, inherently validated by type.
    }
    
    private void CheckFirstName()
        {
            if (string.IsNullOrWhiteSpace(_client.FirstName))
            {
                ValidationHandler.Append(new Error("O nome não pode estar vazio."));
            }
            else if (_client.FirstName.Length > 100)
            {
                ValidationHandler.Append(new Error("O nome não pode exceder 100 caracteres."));
            }
        }

        private void CheckLastName()
        {
            if (string.IsNullOrWhiteSpace(_client.LastName))
            {
                ValidationHandler.Append(new Error("O sobrenome não pode estar vazio."));
            }
            else if (_client.LastName.Length > 155)
            {
                ValidationHandler.Append(new Error("O sobrenome não pode exceder 155 caracteres."));
            }
        }

        private void CheckEmail()
        {
            if (string.IsNullOrWhiteSpace(_client.Email))
            {
                ValidationHandler.Append(new Error("O e-mail não pode estar vazio."));
            }
            else if (_client.Email.Length > 255)
            {
                ValidationHandler.Append(new Error("O e-mail não pode exceder 255 caracteres."));
            }
            // Example of a very basic email format check.
            // For production, consider a more robust validation library or a refined Regex.
            // else if (!Regex.IsMatch(_client.Email, @"^[^@\s]+@[^@\s]+\.[^@\s\.]+$", RegexOptions.IgnoreCase))
            // {
            //     ValidationHandler.Append(new Error("Formato de e-mail inválido."));
            // }
        }

        private void CheckPhone()
        {
            if (string.IsNullOrWhiteSpace(_client.PhoneNumber))
            {
                ValidationHandler.Append(new Error("O telefone não pode estar vazio."));
            }
            else if (_client.PhoneNumber.Length > 20)
            {
                ValidationHandler.Append(new Error("O telefone não pode exceder 20 caracteres."));
            }
        }

        private void CheckPasswordHash()
        {
            if (string.IsNullOrWhiteSpace(_client.PasswordHash))
            {
                ValidationHandler.Append(new Error("O hash da senha não pode estar vazio."));
            }
            else if (_client.PasswordHash.Length > 255)
            {
                ValidationHandler.Append(new Error("O hash da senha excede o limite de 255 caracteres."));
            }
        }

        private void CheckCpf()
        {
            // CPF is nullable in the database
            if (!string.IsNullOrEmpty(_client.Cpf))
            {
                if (_client.Cpf.Length != 11)
                {
                    ValidationHandler.Append(new Error("O CPF deve conter 11 dígitos."));
                }
                else if (!_client.Cpf.All(char.IsDigit))
                {
                    ValidationHandler.Append(new Error("O CPF deve conter apenas dígitos."));
                }
                // Consider adding a full CPF validation algorithm (checksum digits)
                // or using a dedicated Value Object for CPF.
            }
        }
}