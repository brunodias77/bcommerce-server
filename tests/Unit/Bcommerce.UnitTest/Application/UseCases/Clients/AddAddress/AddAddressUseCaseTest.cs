using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Customers.Clients.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.AddAddress
{
    [Collection(nameof(AddAddressUseCaseTestFixture))]
    public class AddAddressUseCaseTest
    {
        private readonly AddAddressUseCaseTestFixture _fixture;

        public AddAddressUseCaseTest(AddAddressUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.AddressRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
            _fixture.LoggedUserMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Adicionar Endereço com Sucesso")]
        [Trait("Application", "AddAddress - UseCase")]
        public async Task Execute_WhenInputIsValid_ShouldAddAddressAndCommit()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Street.Should().Be(input.Street);
            result.Value.ClientId.Should().Be(clientId);

            // CORREÇÃO: Verifica se o repositório de Endereço foi chamado corretamente
            _fixture.AddressRepositoryMock.Verify(r => r.AddAsync(
                It.Is<Address>(a => a.ClientId == clientId && a.PostalCode == input.PostalCode),
                It.IsAny<CancellationToken>()), 
                Times.Once
            );
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Adicionar Endereço com CEP Inválido")]
        [Trait("Application", "AddAddress - UseCase")]
        public async Task Execute_WhenInputIsInvalid_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            // Input inválido (CEP curto)
            var input = _fixture.GetValidInput() with { PostalCode = "123" };
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "'PostalCode' deve ter 8 dígitos.");
            
            // Garante que nenhuma operação de escrita ocorreu
            _fixture.AddressRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Deve Fazer Rollback em Caso de Erro no Repositório")]
        [Trait("Application", "AddAddress - UseCase")]
        public async Task Execute_WhenRepositoryThrows_ShouldReturnErrorAndRollback()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB Error"));
            
            // Act
            var result = await useCase.Execute(input);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Erro ao salvar o endereço.");
            _fixture.UnitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}