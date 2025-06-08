using Bcomerce.Application.UseCases.Clients.ListAddresses;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.UnitTest.Common; // Vamos precisar de um AddressBuilder
using FluentAssertions;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class ListMyAddressesUseCaseUnitTests
{
    private readonly Mock<ILoggedUser> _loggedUserMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly ListMyAddressesUseCase _useCase;
    
    public ListMyAddressesUseCaseUnitTests()
    {
        _loggedUserMock = new Mock<ILoggedUser>();
        _addressRepositoryMock = new Mock<IAddressRepository>();

        _useCase = new ListMyAddressesUseCase(
            _loggedUserMock.Object,
            _addressRepositoryMock.Object
        );
    }
    
    [Fact(DisplayName = "Deve Retornar a Lista de Endereços do Usuário Logado")]
    public async Task Execute_WhenUserHasAddresses_ShouldReturnAddressList()
    {
        // Arrange (Organizar)
        var fakeClientId = Guid.NewGuid();
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(fakeClientId);

        // Criamos uma lista de endereços falsos para o repositório retornar
        var fakeAddresses = new List<Address>
        {
            AddressBuilder.New().WithClientId(fakeClientId).Build(),
            AddressBuilder.New().WithClientId(fakeClientId).Build()
        };

        // PONTO CHAVE: Configura o repositório para retornar a lista de endereços
        // quando for chamado com o ID do usuário logado.
        _addressRepositoryMock
            .Setup(r => r.GetByClientIdAsync(fakeClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeAddresses);

        // Act (Agir)
        var result = await _useCase.Execute(null);

        // Assert (Verificar)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.First().ClientId.Should().Be(fakeClientId);

        // Verifica se o método do repositório foi chamado exatamente uma vez
        // com o ID correto do usuário logado.
        _addressRepositoryMock.Verify(
            r => r.GetByClientIdAsync(fakeClientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
    
    [Fact(DisplayName = "Deve Retornar uma Lista Vazia se o Usuário Não Tiver Endereços")]
    public async Task Execute_WhenUserHasNoAddresses_ShouldReturnEmptyList()
    {
        // Arrange
        var fakeClientId = Guid.NewGuid();
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(fakeClientId);

        // Configura o repositório para retornar uma lista vazia
        _addressRepositoryMock
            .Setup(r => r.GetByClientIdAsync(fakeClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Address>());

        // Act
        var result = await _useCase.Execute(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}