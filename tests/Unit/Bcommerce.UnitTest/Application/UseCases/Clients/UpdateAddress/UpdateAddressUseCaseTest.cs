using Bcommerce.Domain.Customers.Clients.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.UpdateAddress
{
    [Collection(nameof(UpdateAddressUseCaseTestFixture))]
    public class UpdateAddressUseCaseTest
    {
        private readonly UpdateAddressUseCaseTestFixture _fixture;

        public UpdateAddressUseCaseTest(UpdateAddressUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.AddressRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
            _fixture.LoggedUserMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Atualizar Endereço com Sucesso")]
        [Trait("Application", "UpdateAddress - UseCase")]
        public async Task Execute_WhenAddressExistsAndBelongsToUser_ShouldUpdateSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var address = _fixture.CreateValidAddress(clientId);
            var input = _fixture.GetValidInput(address.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Street.Should().Be(input.Street);
            result.Value.Complement.Should().Be(input.Complement);

            _fixture.AddressRepositoryMock.Verify(r => r.UpdateAsync(
                It.Is<Address>(a => a.Id == address.Id && a.Street == input.Street), 
                It.IsAny<CancellationToken>()), 
                Times.Once
            );
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Atualizar Endereço Inexistente")]
        [Trait("Application", "UpdateAddress - UseCase")]
        public async Task Execute_WhenAddressDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var nonExistentAddressId = Guid.NewGuid();
            var input = _fixture.GetValidInput(nonExistentAddressId);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(nonExistentAddressId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Endereço não encontrado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Atualizar Endereço de Outro Usuário")]
        [Trait("Application", "UpdateAddress - UseCase")]
        public async Task Execute_WhenAddressDoesNotBelongToUser_ShouldReturnError()
        {
            // Arrange
            var loggedUserId = Guid.NewGuid();
            var addressOwnerId = Guid.NewGuid(); // ID do verdadeiro dono do endereço
            var address = _fixture.CreateValidAddress(addressOwnerId);
            var input = _fixture.GetValidInput(address.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(loggedUserId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Endereço não encontrado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}