using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Events;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using FluentAssertions;

namespace Bcommerce.UnitTest.Domain.Entities.Clients;

// 2. A classe agora implementa IClassFixture<ClientTestFixture>, informando ao xUnit
//    que ela depende desta fixture. O xUnit cuidará de criar uma instância única
//    da fixture e injetá-la no construtor.
[Collection("ClientTests")]
public class ClientUnitTest
{
    // CORREÇÃO: O nome da fixture foi padronizado para ClientTestFixture.
    private readonly ClientTestFixture _fixture;

    // O construtor recebe a fixture que o xUnit injeta automaticamente.
    public ClientUnitTest(ClientTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    // --- Testes para o método de fábrica NewClient ---
    
    [Fact(DisplayName = "Deve Criar Cliente com Dados Válidos")]
    [Trait("Domain", "Client - Aggregate")]
    public void NewClient_WithValidData_ShouldCreateClientAndRaiseEvent()
    {
        // Arrange
        var handler = Notification.Create();
        // 4. Usamos o método auxiliar da fixture para obter dados de teste válidos.
        //    Isso torna o teste mais limpo e centraliza a lógica de criação de dados.
        var (firstName, lastName, validEmail, phoneNumber, passwordHash) = _fixture.GetValidClientInputData();

        // Act
        var client = Client.NewClient(
            firstName,
            lastName,
            validEmail,
            phoneNumber,
            passwordHash,
            null,
            null,
            true,
            handler
        );

        // Assert
        handler.HasError().Should().BeFalse();
        client.Should().NotBeNull();
        client.FirstName.Should().Be(firstName);
        client.Email.Value.Should().Be(validEmail);
        client.Status.Should().Be(ClientStatus.Active);
        client.Events.Should().HaveCount(1);
        client.Events.First().Should().BeOfType<ClientCreatedEvent>();
    }

    
    [Theory(DisplayName = "Não Deve Criar Cliente com Dados Inválidos")]
    [Trait("Domain", "Client - Aggregate")]
    [InlineData("", "Sobrenome", "email@valido.com", "'FirstName' é obrigatório.")]
    [InlineData("Nome", "", "email@valido.com", "'LastName' é obrigatório.")]
    [InlineData("Nome", "Sobrenome", "email_invalido", "Endereço de e-mail inválido.")]
    public void NewClient_WithInvalidData_ShouldReturnErrorAndNotRaiseEvent(
        string firstName, string lastName, string email, string expectedErrorMessage)
    {
        // Arrange
        var handler = Notification.Create();

        // Act
        var client = Client.NewClient(
            firstName,
            lastName,
            email,
            "phone",
            "password",
            null,
            null,
            false,
            handler
        );

        // Assert
        handler.HasError().Should().BeTrue();
        handler.GetErrors().Should().Contain(e => e.Message.Contains(expectedErrorMessage));
        client.Events.Should().BeEmpty();
    }

    [Fact(DisplayName = "Deve Verificar E-mail e Disparar Evento")]
    [Trait("Domain", "Client - Aggregate")]
    public void VerifyEmail_WhenEmailIsNotVerified_ShouldSetVerificationDateAndRaiseEvent()
    {
        // Arrange
        // 5. Usamos o método da fixture para obter uma entidade cliente já válida.
        var client = _fixture.CreateValidClient();
        var initialUpdateDate = client.UpdatedAt;

        // Act
        client.VerifyEmail();

        // Assert
        client.EmailVerified.Should().NotBeNull();
        client.EmailVerified.Should().BeOnOrAfter(initialUpdateDate);
        client.UpdatedAt.Should().BeAfter(initialUpdateDate);
        client.Events.Should().HaveCount(1);
        client.Events.First().Should().BeOfType<ClientEmailVerifiedEvent>();
    }
    
    [Fact(DisplayName = "Não Deve Fazer Nada se E-mail Já Estiver Verificado")]
    [Trait("Domain", "Client - Aggregate")]
    public void VerifyEmail_WhenEmailIsAlreadyVerified_ShouldBeIdempotent()
    {
        // Arrange
        var client = _fixture.CreateValidClient();
        client.VerifyEmail(); // Primeira verificação
        
        var firstVerificationDate = client.EmailVerified;
        var firstUpdateDate = client.UpdatedAt;
        client.ClearEvents();

        // Act
        client.VerifyEmail(); // Segunda verificação (deve ser idempotente)

        // Assert
        client.EmailVerified.Should().Be(firstVerificationDate);
        client.UpdatedAt.Should().Be(firstUpdateDate);
        client.Events.Should().BeEmpty();
    }
}