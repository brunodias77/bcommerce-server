using Bcomerce.Application.UseCases.Clients.DeleteAddress;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.UnitTest.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients;

public class DeleteAddressUseCaseUnitTests
{
    private readonly Mock<ILoggedUser> _loggedUserMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteAddressUseCase _useCase;
    
    public DeleteAddressUseCaseUnitTests()
    {
        _loggedUserMock = new Mock<ILoggedUser>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        // Supondo que o construtor do DeleteAddressUseCase receba estas dependências
        _useCase = new DeleteAddressUseCase(
            _loggedUserMock.Object,
            _addressRepositoryMock.Object,
            _uowMock.Object
        );
    }
    
    [Fact(DisplayName = "Deve Remover Endereço com Sucesso")]
    public async Task Execute_WithValidDataAndOwnership_ShouldSoftDeleteAddressAndCommit()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var input = new DeleteAddressInput(addressId);

        var existingAddress = AddressBuilder.New()
            .WithId(addressId)
            .WithClientId(clientId)
            .Build();

        _loggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
        _addressRepositoryMock
            .Setup(r => r.GetByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAddress);

        // Act
        var result = await _useCase.Execute(input);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verifica se o método Update foi chamado com um endereço cujo DeletedAt não é nulo (soft delete)
        _addressRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Address>(a => a.Id == addressId && a.DeletedAt != null),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        
        // Verifica se a transação foi commitada
        _uowMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Não Deve Remover Endereço que Não Pertence ao Usuário Logado")]
    public async Task Execute_WhenAddressDoesNotBelongToUser_ShouldReturnNotFoundError()
    {
        // Arrange
        var loggedInClientId = Guid.NewGuid();
        var addressOwnerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var input = new DeleteAddressInput(addressId);

        var addressFromAnotherUser = AddressBuilder.New()
            .WithId(addressId)
            .WithClientId(addressOwnerId) // Endereço pertence a outro cliente
            .Build();

        _loggedUserMock.Setup(u => u.GetClientId()).Returns(loggedInClientId);
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

    [Fact(DisplayName = "Não Deve Remover Endereço se Ele Não For Encontrado")]
    public async Task Execute_WhenAddressIsNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var input = new DeleteAddressInput(addressId);
        
        _loggedUserMock.Setup(u => u.GetClientId()).Returns(Guid.NewGuid());
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