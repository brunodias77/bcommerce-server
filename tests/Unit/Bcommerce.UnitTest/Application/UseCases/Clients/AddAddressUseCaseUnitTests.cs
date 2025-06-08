using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.UnitTest.Common;
using Bogus;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class AddAddressUseCaseUnitTests
{
    private readonly Mock<ILoggedUser> _loggedUserMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly AddAddressUseCase _useCase;
    private readonly Faker _faker = FakerGenerator.Faker;
    
    public AddAddressUseCaseUnitTests()
    {
        _loggedUserMock = new Mock<ILoggedUser>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _useCase = new AddAddressUseCase(
            _loggedUserMock.Object,
            _addressRepositoryMock.Object,
            _uowMock.Object
        );
    }
    
    [Fact(DisplayName = "Deve Adicionar Endereço com Sucesso com Dados Válidos")]
    public async Task Execute_WithValidData_ShouldAddAddressAndCommit()
    {
        // Arrange (Organizar)
        var input = new AddAddressInput(
            Type: Bcommerce.Domain.Clients.enums.AddressType.Shipping,
            PostalCode: _faker.Address.ZipCode("########"),
            Street: _faker.Address.StreetName(),
            Number: _faker.Address.BuildingNumber(),
            Complement: _faker.Address.SecondaryAddress(),
            Neighborhood: _faker.Address.StreetSuffix(),
            City: _faker.Address.City(),
            StateCode: _faker.Address.StateAbbr(),
            IsDefault: true
        );

        // Simula que existe um usuário logado com um ID específico
        var fakeClientId = Guid.NewGuid();
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(fakeClientId);

        // Act (Agir)
        var result = await _useCase.Execute(input);

        // Assert (Verificar)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ClientId.Should().Be(fakeClientId);
        result.Value.Street.Should().Be(input.Street);

        // Verifica se o repositório foi chamado para adicionar o endereço
        _addressRepositoryMock.Verify(
            repo => repo.AddAsync(
                It.Is<Bcommerce.Domain.Clients.Address>(a => a.ClientId == fakeClientId && a.Street == input.Street),
                It.IsAny<CancellationToken>()
            ), 
            Times.Once
        );
    
        // Verifica se a transação foi commitada
        _uowMock.Verify(uow => uow.Commit(), Times.Once);
    }
    
    [Fact(DisplayName = "Não Deve Adicionar Endereço se a Validação de Domínio Falhar")]
    public async Task Execute_WithInvalidData_ShouldReturnDomainValidationError()
    {
        // Arrange
        var input = new AddAddressInput(
            Type: Bcommerce.Domain.Clients.enums.AddressType.Shipping,
            PostalCode: "CEP_INVALIDO", // CEP inválido para forçar o erro
            Street: _faker.Address.StreetName(),
            Number: _faker.Address.BuildingNumber(),
            null, null, null, null, false
        );
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(Guid.NewGuid());

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.GetErrors().Should().Contain(e => e.Message.Contains("CEP deve conter 8 dígitos"));
    
        // Garante que nenhuma operação de escrita foi tentada
        _addressRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Bcommerce.Domain.Clients.Address>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(uow => uow.Commit(), Times.Never);
    }
}