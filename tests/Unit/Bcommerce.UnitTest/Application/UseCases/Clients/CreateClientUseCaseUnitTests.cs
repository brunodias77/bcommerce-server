using Bcomerce.Application.UseCases.Clients.Create;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Events;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.UnitTest.Common;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class CreateClientUseCaseUnitTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IPasswordEncripter> _passwordEncrypterMock;
    private readonly Mock<IDomainEventPublisher> _publisherMock;
    private readonly CreateClientUseCase _useCase;
    private readonly Faker _faker = FakerGenerator.Faker;

    public CreateClientUseCaseUnitTests()
    {
        _clientRepositoryMock = new Mock<IClientRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _passwordEncrypterMock = new Mock<IPasswordEncripter>();
        _publisherMock = new Mock<IDomainEventPublisher>();
        var loggerMock = new Mock<ILogger<CreateClientUseCase>>();
        
        _useCase = new CreateClientUseCase(
            _clientRepositoryMock.Object,
            _uowMock.Object,
            _passwordEncrypterMock.Object,
            loggerMock.Object,
            _publisherMock.Object
        );
    }
    
    // --- Teste de Sucesso (Happy Path) ---
    [Fact(DisplayName = "Deve Criar e Salvar Cliente com Sucesso")]
    public async Task Execute_WhenAllDataIsValid_ShouldSucceed()
    {
        // Arrange
        var input = new CreateClientInput(
            _faker.Person.FirstName, _faker.Person.LastName,
            _faker.Person.Email, _faker.Person.Phone, "ValidPassword123!", true
        );
        _clientRepositoryMock
            .Setup(repo => repo.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);
        _passwordEncrypterMock
            .Setup(p => p.Encrypt(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _clientRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(uow => uow.Commit(), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Testes de Falha (Cenários de Erro) ---

    [Fact(DisplayName = "Não Deve Criar Cliente se o E-mail já Existir")]
    public async Task Execute_WhenEmailAlreadyExists_ShouldFail()
    {
        // Arrange
        var input = new CreateClientInput(_faker.Person.FirstName, _faker.Person.LastName, _faker.Person.Email, _faker.Person.Phone, "ValidPassword123!", true);
        var existingClient = Client.NewClient(input.FirstName, input.LastName, input.Email, input.PhoneNumber, "hash", null, null, true, Bcommerce.Domain.Validations.Handlers.Notification.Create());
    
        _passwordEncrypterMock
            .Setup(p => p.Encrypt(It.IsAny<string>()))
            .Returns("a_valid_mocked_hash");

        _clientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message.Contains("e-mail informado já está em uso"));
        _uowMock.Verify(uow => uow.Commit(), Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<ClientCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Theory(DisplayName = "Não Deve Criar Cliente com Senha Inválida")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_WithInvalidPassword_ShouldFailEarly(string invalidPassword)
    {
        // Arrange
        var input = new CreateClientInput(
            _faker.Person.FirstName, _faker.Person.LastName,
            _faker.Person.Email, _faker.Person.Phone, invalidPassword, true
        );

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message == "A senha nao pode estar vazia!");
        
        // Garante que nenhuma operação de banco ou encriptação foi tentada
        _clientRepositoryMock.Verify(repo => repo.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordEncrypterMock.Verify(p => p.Encrypt(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Criar Cliente se a Validação de Domínio Falhar")]
    public async Task Execute_WhenDomainValidationFails_ShouldFail()
    {
        // Arrange
        var input = new CreateClientInput(
            "", // Nome inválido para forçar erro de domínio
            _faker.Person.LastName, _faker.Person.Email, _faker.Person.Phone, "ValidPassword123!", true
        );
        _clientRepositoryMock
            .Setup(repo => repo.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);
         _passwordEncrypterMock
            .Setup(p => p.Encrypt(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message.Contains("nome não pode estar vazio"));
        _clientRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact(DisplayName = "Deve Retornar Erro e Fazer Rollback em Caso de Exceção na Escrita")]
    public async Task Execute_WhenWriteRepositoryThrowsException_ShouldRollbackAndFail()
    {
        // Arrange
        var input = new CreateClientInput(_faker.Person.FirstName, _faker.Person.LastName, _faker.Person.Email, _faker.Person.Phone, "ValidPassword123!", true);
        _clientRepositoryMock
            .Setup(repo => repo.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);
        _passwordEncrypterMock
            .Setup(p => p.Encrypt(It.IsAny<string>()))
            .Returns("hashed_password");
        _clientRepositoryMock
            .Setup(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Erro de banco simulado"));
        // <<< ADICIONE ESTA LINHA PARA CONFIGURAR A PROPRIEDADE DO MOCK >>>
        _uowMock.Setup(uow => uow.HasActiveTransaction).Returns(true);


        // Act
        var result = await _useCase.Execute(input);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => e.Message.Contains("Erro ao salvar o cliente."));
        _uowMock.Verify(uow => uow.Rollback(), Times.Once);
        _uowMock.Verify(uow => uow.Commit(), Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<ClientCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}