using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Events;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Create;


[Collection(nameof(CreateClientUseCaseTestFixture))]
public class CreateClientUseCaseTest
{
    private readonly CreateClientUseCaseTestFixture _fixture;

    public CreateClientUseCaseTest(CreateClientUseCaseTestFixture fixture)
    {
        _fixture = fixture;
        
        // CORREÇÃO: Limpa o histórico de invocações dos mocks antes de cada teste.
        // Isso garante que cada teste seja executado em um estado isolado, prevenindo
        // que as chamadas de um teste interfiram na verificação de outro.
        _fixture.ClientRepositoryMock.Invocations.Clear();
        _fixture.UnitOfWorkMock.Invocations.Clear();
        _fixture.PasswordEncripterMock.Invocations.Clear();
        _fixture.DomainEventPublisherMock.Invocations.Clear();
        _fixture.LoggerMock.Invocations.Clear();
        
    }
    
    [Fact(DisplayName = "Deve Criar Cliente com Sucesso")]
    [Trait("Application", "CreateClient - UseCase")]
    public async Task Execute_WhenInputIsValid_ShouldCreateClientSuccessfully()
    {
        // Arrange
        var input = _fixture.GetValidCreateClientInput();
        var useCase = _fixture.CreateUseCase();

        // Configura o mock do repositório para simular que o e-mail não existe
        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null); // Retorna nulo, indicando que o e-mail está disponível

        // Configura o mock do encriptador de senha
        _fixture.PasswordEncripterMock
            .Setup(enc => enc.Encrypt(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await useCase.Execute(input);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be(input.FirstName);
        result.Value.Email.Should().Be(input.Email);

        // Verifica se os métodos corretos foram chamados nos mocks (verificação de comportamento)
        _fixture.ClientRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);

        // CORREÇÃO: A verificação agora corresponde à chamada real.
        // O método é chamado com o tipo genérico base `DomainEvent`,
        // mas o argumento passado é uma instância de `ClientCreatedEvent`.
        _fixture.DomainEventPublisherMock.Verify(
            pub => pub.PublishAsync<DomainEvent>(
                It.IsAny<ClientCreatedEvent>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
    
    [Fact(DisplayName = "Não Deve Criar Cliente se E-mail Já Existir")]
    [Trait("Application", "CreateClient - UseCase")]
    public async Task Execute_WhenEmailAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var input = _fixture.GetValidCreateClientInput();
        var useCase = _fixture.CreateUseCase();
        
        // CORREÇÃO: Em vez de Mock.Of<Client>(), usamos a fixture para criar uma instância
        // real e válida da entidade Client. Isso resolve o erro de construtor não encontrado.
        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.CreateValidClient()); 

        // Act
        var result = await useCase.Execute(input);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.GetErrors().Should().Contain(e => e.Message == "O e-mail informado já está em uso.");
        
        _fixture.ClientRepositoryMock.Verify(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        _fixture.DomainEventPublisherMock.Verify(pub => pub.PublishAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    
    [Theory(DisplayName = "Não Deve Criar Cliente com Input Inválido")]
    [Trait("Application", "CreateClient - UseCase")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task Execute_WhenPasswordIsInvalid_ShouldReturnError(string invalidPassword)
    {
        // Arrange
        var input = _fixture.GetValidCreateClientInput() with { Password = invalidPassword };
        var useCase = _fixture.CreateUseCase();
        
        // Act
        var result = await useCase.Execute(input);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.GetErrors().Should().Contain(e => e.Message == "A senha não pode estar vazia.");
    }
    
    
    [Fact(DisplayName = "Deve Lidar com Exceção do Repositório e Fazer Rollback")]
    [Trait("Application", "CreateClient - UseCase")]
    public async Task Execute_WhenRepositoryThrowsException_ShouldReturnErrorAndRollback()
    {
        // Arrange
        var input = _fixture.GetValidCreateClientInput();
        var useCase = _fixture.CreateUseCase();
        var exception = new Exception("Database connection failed");

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);

        _fixture.ClientRepositoryMock
            .Setup(repo => repo.Insert(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
            
        // CORREÇÃO: Configuramos o mock para retornar 'true' quando a propriedade
        // HasActiveTransaction for acessada. Isso simula o estado de uma transação
        // que foi iniciada mas falhou antes do Commit.
        _fixture.UnitOfWorkMock.Setup(uow => uow.HasActiveTransaction).Returns(true);
        
        // Act
        var result = await useCase.Execute(input);
    
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().Contain(e => 
            e.Message == "Não foi possível processar seu registro. Tente novamente mais tarde.");
    
        // A verificação de Rollback agora passará, pois HasActiveTransaction retornará 'true'.
        _fixture.UnitOfWorkMock.Verify(uow => uow.Rollback(), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
    }
    
}