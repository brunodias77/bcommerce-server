using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Customers.Clients.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.ListAddresses
{
    [Collection(nameof(ListMyAddressesUseCaseTestFixture))]
    public class ListMyAddressesUseCaseTest
    {
        private readonly ListMyAddressesUseCaseTestFixture _fixture;

        public ListMyAddressesUseCaseTest(ListMyAddressesUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.AddressRepositoryMock.Invocations.Clear();
            _fixture.LoggedUserMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Listar Endereços com Sucesso")]
        [Trait("Application", "ListMyAddresses - UseCase")]
        public async Task Execute_WhenClientHasAddresses_ShouldReturnAddressList()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var addresses = _fixture.CreateValidAddressList(clientId, 3);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(addresses);

            // Act
            // CORREÇÃO: Passando 'null' como input, conforme a interface do UseCase
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(3);
            result.Value.First().Street.Should().Be(addresses.First().Street);

            _fixture.AddressRepositoryMock.Verify(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Deve Retornar Lista Vazia para Cliente Sem Endereços")]
        [Trait("Application", "ListMyAddresses - UseCase")]
        public async Task Execute_WhenClientHasNoAddresses_ShouldReturnEmptyList()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.AddressRepositoryMock
                .Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Address>()); // Retorna uma lista vazia

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();

            _fixture.AddressRepositoryMock.Verify(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}