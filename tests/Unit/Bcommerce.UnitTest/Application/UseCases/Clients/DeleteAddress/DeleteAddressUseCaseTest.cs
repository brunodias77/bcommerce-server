using Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;
using Bcommerce.Domain.Customers.Clients.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.DeleteAddress
{
    [Collection(nameof(DeleteAddressUseCaseTestFixture))]
    public class DeleteAddressUseCaseTest
    {
        private readonly DeleteAddressUseCaseTestFixture _fixture;

        public DeleteAddressUseCaseTest(DeleteAddressUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.AddressRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
            _fixture.LoggedUserMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Excluir Endereço com Sucesso")]
        [Trait("Application", "DeleteAddress - UseCase")]
        public async Task Execute_WhenAddressExistsAndBelongsToUser_ShouldDeleteSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var address = _fixture.CreateValidAddress(clientId);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            // Act
            var result = await useCase.Execute(new DeleteAddressInput(address.Id));

            // Assert
            result.IsSuccess.Should().BeTrue();
            
            // Verifica se o repositório foi chamado para atualizar o endereço,
            // e que o endereço passado para ele está com o soft-delete marcado.
            _fixture.AddressRepositoryMock.Verify(r => r.UpdateAsync(
                It.Is<Address>(a => a.Id == address.Id && a.DeletedAt != null), 
                It.IsAny<CancellationToken>()), 
                Times.Once
            );
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Excluir Endereço Inexistente")]
        [Trait("Application", "DeleteAddress - UseCase")]
        public async Task Execute_WhenAddressNotFound_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var nonExistentAddressId = Guid.NewGuid();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentAddressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address)null);

            // Act
            var result = await useCase.Execute(new DeleteAddressInput(nonExistentAddressId));

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Endereço não encontrado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Excluir Endereço de Outro Usuário")]
        [Trait("Application", "DeleteAddress - UseCase")]
        public async Task Execute_WhenAddressDoesNotBelongToUser_ShouldReturnError()
        {
            // Arrange
            var loggedUserId = Guid.NewGuid();
            var anotherClientId = Guid.NewGuid();
            var addressOfAnotherUser = _fixture.CreateValidAddress(anotherClientId); // Endereço pertence a outro cliente
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(loggedUserId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(addressOfAnotherUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(addressOfAnotherUser);

            // Act
            var result = await useCase.Execute(new DeleteAddressInput(addressOfAnotherUser.Id));

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Endereço não encontrado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}