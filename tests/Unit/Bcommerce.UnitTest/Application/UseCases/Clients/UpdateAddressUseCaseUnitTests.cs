using Bcomerce.Application.UseCases.Clients.UpdateAddress;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.UnitTest.Common;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class UpdateAddressUseCaseUnitTests
{
    private readonly Mock<ILoggedUser> _loggedUserMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateAddressUseCase _useCase;
    private readonly Faker _faker = new("pt_BR");

    public UpdateAddressUseCaseUnitTests()
    {
        _loggedUserMock = new Mock<ILoggedUser>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<UpdateAddressUseCase>>();

        _useCase = new UpdateAddressUseCase(
            _loggedUserMock.Object,
            _addressRepositoryMock.Object,
            _uowMock.Object
        );
    }
    
    [Fact(DisplayName = "Deve Atualizar Endereço com Sucesso com Dados Válidos")]
    public async Task Execute_WithValidDataAndOwnership_ShouldUpdateAndCommit()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var addressId = Guid.NewGuid();

        var input = new UpdateAddressInput(
            addressId, AddressType.Billing, _faker.Address.ZipCode("########"),
            "Rua Nova", _faker.Address.BuildingNumber(), null, "Bairro Novo",
            "Cidade Nova", "SP", true
        );

        var existingAddress = AddressBuilder.New()
            .WithId(addressId) // Garante que o ID é o mesmo
            .WithClientId(clientId) // Garante que o endereço pertence ao usuário
            .Build();

        _loggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
        _addressRepositoryMock
            .Setup(r => r.GetByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAddress);

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Street.Should().Be("Rua Nova");
        result.Value.Type.Should().Be(AddressType.Billing);

        _addressRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Não Deve Atualizar Endereço que Não Pertence ao Usuário Logado")]
    public async Task Execute_WhenAddressDoesNotBelongToUser_ShouldReturnNotFoundError()
    {
        // Arrange
        var loggedInClientId = Guid.NewGuid(); // ID do usuário que está logado
        var addressOwnerId = Guid.NewGuid();   // ID do verdadeiro dono do endereço
        var addressId = Guid.NewGuid();
        
        var input = new UpdateAddressInput(addressId, AddressType.Shipping, "12345678", "Rua", "123", null, "Bairro", "Cidade", "SP", false);

        // O endereço que o repositório retorna pertence a OUTRO usuário
        var addressFromAnotherUser = AddressBuilder.New()
            .WithId(addressId)
            .WithClientId(addressOwnerId) 
            .Build();

        // O usuário logado é o 'loggedInClientId'
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(loggedInClientId);
        
        // O repositório encontra o endereço, mas ele tem o 'addressOwnerId'
        _addressRepositoryMock
            .Setup(r => r.GetByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addressFromAnotherUser);

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().ContainSingle()
            .Which.Message.Should().Be("Endereço não encontrado.");
        
        // Garante que nenhuma operação de escrita foi tentada
        _addressRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(uow => uow.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Não Deve Atualizar Endereço se Ele Não For Encontrado")]
    public async Task Execute_WhenAddressIsNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var input = new UpdateAddressInput(addressId, AddressType.Shipping, "12345678", "Rua", "123", null, "Bairro", "Cidade", "SP", false);
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(Guid.NewGuid());
        
        // Configura o repositório para não encontrar nenhum endereço com o ID fornecido
        _addressRepositoryMock
            .Setup(r => r.GetByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address)null);

        // Act
        var result = await _useCase.Execute(input);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.GetErrors().Should().ContainSingle()
            .Which.Message.Should().Be("Endereço não encontrado.");
    }
}