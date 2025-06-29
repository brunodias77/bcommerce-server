using Bcommerce.Domain.Customers.Clients;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.GetMyProfile
{
    [Collection(nameof(GetMyProfileUseCaseTestFixture))]
    public class GetMyProfileUseCaseTest
    {
        private readonly GetMyProfileUseCaseTestFixture _fixture;

        public GetMyProfileUseCaseTest(GetMyProfileUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ClientRepositoryMock.Invocations.Clear();
            _fixture.LoggedUserMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Obter Perfil com Sucesso")]
        [Trait("Application", "GetMyProfile - UseCase")]
        public async Task Execute_WhenClientExists_ShouldReturnProfile()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(client.Id);
            _fixture.ClientRepositoryMock
                .Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            // CORREÇÃO: Passando 'null' para satisfazer a assinatura do método.
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(client.Id);
            result.Value.Email.Should().Be(client.Email.Value);

            _fixture.LoggedUserMock.Verify(u => u.GetClientId(), Times.Once);
            _fixture.ClientRepositoryMock.Verify(r => r.Get(client.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Obter Perfil se Cliente Não For Encontrado")]
        [Trait("Application", "GetMyProfile - UseCase")]
        public async Task Execute_WhenClientNotFound_ShouldReturnError()
        {
            // Arrange
            var useCase = _fixture.CreateUseCase();
            var clientId = Guid.NewGuid();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.ClientRepositoryMock
                .Setup(r => r.Get(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client)null);

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Usuário não encontrado.");
            
            _fixture.LoggedUserMock.Verify(u => u.GetClientId(), Times.Once);
            _fixture.ClientRepositoryMock.Verify(r => r.Get(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}