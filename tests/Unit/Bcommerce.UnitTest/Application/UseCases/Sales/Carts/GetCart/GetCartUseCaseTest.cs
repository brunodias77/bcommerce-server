using Bcommerce.Domain.Sales.Carts;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.GetCart;

[Collection(nameof(GetCartUseCaseTestFixture))]
    public class GetCartUseCaseTest
    {
        private readonly GetCartUseCaseTestFixture _fixture;

        public GetCartUseCaseTest(GetCartUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.CartRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Retornar Carrinho Existente com Sucesso")]
        [Trait("Application", "GetCart - UseCase")]
        public async Task Execute_WhenCartExists_ShouldReturnCart()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var existingCart = _fixture.CreateValidCart(clientId);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock
                .Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCart);

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.CartId.Should().Be(existingCart.Id);
            result.Value.Items.Should().HaveCount(1);
            _fixture.CartRepositoryMock.Verify(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "Deve Criar e Retornar Novo Carrinho se Não Existir")]
        [Trait("Application", "GetCart - UseCase")]
        public async Task Execute_WhenCartDoesNotExist_ShouldCreateAndReturnNewCart()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock
                .Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cart)null); // Simula que o carrinho não foi encontrado

            // Act
            var result = await useCase.Execute(null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.ClientId.Should().Be(clientId);
            result.Value.Items.Should().BeEmpty();
            _fixture.CartRepositoryMock.Verify(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }
    }