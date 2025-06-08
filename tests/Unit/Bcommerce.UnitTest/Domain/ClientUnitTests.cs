using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Events;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.UnitTest.Common;
using Bogus;
using FluentAssertions;

namespace Bcommerce.UnitTest.Domain;

public class ClientUnitTests
{
    private readonly Faker _faker = FakerGenerator.Faker;

    [Fact(DisplayName = "Deve Criar um Cliente Válido com Dados Corretos")]
    public void NewClient_WithValidData_ShouldCreateSuccessfullyAndRaiseEvent()
    {
        // Arrange
        var notification = Notification.Create();
        var validFirstName = _faker.Person.FirstName;
        var validLastName = _faker.Person.LastName;
        var validEmail = _faker.Person.Email;
        var validPhone = _faker.Person.Phone;
        var validPasswordHash = "a_valid_hashed_password";
        var validCpf = _faker.Random.Replace("###########");

        // Act
        var client = Client.NewClient(
            validFirstName,
            validLastName,
            validEmail,
            validPhone,
            validPasswordHash,
            validCpf,
            null, // date of birth
            true, // newsletter
            notification
        );
        
        // Assert
        notification.HasError().Should().BeFalse();
        client.Should().NotBeNull();
        client.FirstName.Should().Be(validFirstName);
        client.Cpf.Should().Be(validCpf);
        client.Events.Should().ContainSingle()
            .Which.Should().BeOfType<ClientCreatedEvent>();
    }

    [Theory(DisplayName = "Não Deve Criar Cliente com Dados de Domínio Inválidos")]
    // Validações de FirstName
    [InlineData("", "LastName", "email@valido.com", "11999999999", "hash", "O nome não pode estar vazio.")]
    [InlineData("Nome Muito Longo que ultrapassa o limite de cem caracteres estabelecido pela regra de negócio para este campo", "LastName", "email@valido.com", "11999999999", "hash", "O nome não pode exceder 100 caracteres.")]
    // Validações de LastName
    [InlineData("FirstName", "", "email@valido.com", "11999999999", "hash", "O sobrenome não pode estar vazio.")]
    // Validações de Email
    [InlineData("FirstName", "LastName", "", "11999999999", "hash", "O e-mail não pode estar vazio.")]
    // Validações de PhoneNumber
    [InlineData("FirstName", "LastName", "email@valido.com", "", "hash", "O telefone não pode estar vazio.")]
    [InlineData("FirstName", "LastName", "email@valido.com", "123456789012345678901", "hash", "O telefone não pode exceder 20 caracteres.")]
    // Validações de PasswordHash
    [InlineData("FirstName", "LastName", "email@valido.com", "11999999999", "", "O hash da senha não pode estar vazio.")]
    public void NewClient_WithInvalidData_ShouldReturnValidationError(
        string firstName, string lastName, string email, string phoneNumber, string passwordHash, string expectedErrorMessage)
    {
        // Arrange
        var notification = Notification.Create();

        // Act
        Client.NewClient(
            firstName,
            lastName,
            email,
            phoneNumber,
            passwordHash,
            null, null, true,
            notification
        );

        // Assert
        notification.HasError().Should().BeTrue();
        notification.GetErrors().Should().Contain(e => e.Message == expectedErrorMessage);
    }

    [Theory(DisplayName = "Não Deve Criar Cliente com CPF Inválido (se fornecido)")]
    [InlineData("1234567890", "O CPF deve conter 11 dígitos.")]      // CPF curto
    [InlineData("123456789012", "O CPF deve conter 11 dígitos.")]    // CPF longo
    [InlineData("1234567890a", "O CPF deve conter apenas dígitos.")] // CPF com letra
    public void NewClient_WithInvalidCpf_ShouldReturnValidationError(string invalidCpf, string expectedErrorMessage)
    {
        // Arrange
        var notification = Notification.Create();

        // Act
        Client.NewClient(
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email,
            _faker.Person.Phone,
            "hashed_password",
            invalidCpf, // CPF inválido
            null, true,
            notification
        );

        // Assert
        notification.HasError().Should().BeTrue();
        notification.GetErrors().Should().Contain(e => e.Message == expectedErrorMessage);
    }
    // // Usando o Faker da nossa pasta Common para consistência
    // private readonly Faker _faker = FakerGenerator.Faker;
    //
    // [Fact(DisplayName = "Deve Criar um Cliente Válido com Dados Corretos")]
    // public void NewClient_WithValidData_ShouldCreateSuccessfullyAndRaiseEvent()
    // {
    //     // Arrange (Organizar)
    //     var notification = Notification.Create();
    //     var validFirstName = _faker.Person.FirstName;
    //     var validLastName = _faker.Person.LastName;
    //     var validEmail = _faker.Person.Email;
    //     var validPhone = _faker.Person.Phone;
    //     var validPasswordHash = "hashed_password";
    //     
    //     // Act (Agir)
    //     var client = Client.NewClient(
    //         validFirstName,
    //         validLastName,
    //         validEmail,
    //         validPhone,
    //         validPasswordHash,
    //         null, // cpf
    //         null, // date of birth
    //         true, // newsletter
    //         notification
    //     );
    //     
    //     // Assert (Verificar)
    //     notification.HasError().Should().BeFalse();
    //     client.Should().NotBeNull();
    //     client.FirstName.Should().Be(validFirstName);
    //     client.Email.Should().Be(validEmail);
    //     client.Id.Should().NotBeEmpty();
    //     
    //     // Garante que o evento de domínio foi disparado
    //     client.Events.Should().ContainSingle()
    //         .Which.Should().BeOfType<ClientCreatedEvent>();
    // }
    //
    // [Theory(DisplayName = "Não Deve Criar Cliente com Dados Inválidos")]
    // [InlineData("", "Sobrenome", "email@valido.com", "O nome não pode estar vazio.")]
    // [InlineData("NomeMuitoLongo...xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "Sobrenome", "email@valido.com", "O nome não pode exceder 100 caracteres.")]
    // [InlineData("Nome", "", "email@valido.com", "O sobrenome não pode estar vazio.")]
    // [InlineData("Nome", "Sobrenome", "", "O e-mail não pode estar vazio.")]
    // public void NewClient_WithInvalidData_ShouldReturnValidationError(
    //     string firstName, string lastName, string email, string expectedErrorMessage)
    // {
    //     // Arrange
    //     var notification = Notification.Create();
    //
    //     // Act
    //     Client.NewClient(
    //         firstName,
    //         lastName,
    //         email,
    //         _faker.Person.Phone,
    //         "hashed_password",
    //         null, null, true,
    //         notification
    //     );
    //
    //     // Assert
    //     notification.HasError().Should().BeTrue();
    //     notification.GetErrors().Should().Contain(e => e.Message == expectedErrorMessage);
    // }

}