using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;

[Collection(nameof(UpdateCartItemQuantityUseCaseTestFixture))]
    public class UpdateCartItemQuantityUseCaseTest
    {
        private readonly UpdateCartItemQuantityUseCaseTestFixture _fixture;

        public UpdateCartItemQuantityUseCaseTest(UpdateCartItemQuantityUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.CartRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Atualizar Quantidade de Item com Sucesso")]
        [Trait("Application", "UpdateCartItemQuantity - UseCase")]
        public async Task Execute_WhenItemExists_ShouldUpdateQuantityAndCommit()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var cart = _fixture.CreateCartWithItems(clientId);
            var itemToUpdate = cart.Items.First();
            var input = _fixture.GetValidInput(itemToUpdate.Id, 5);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var updatedItem = result.Value.Items.First(i => i.CartItemId == itemToUpdate.Id);
            updatedItem.Quantity.Should().Be(5);
            _fixture.CartRepositoryMock.Verify(r => r.Update(cart, It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Atualizar Quantidade se Item Não Pertence ao Carrinho")]
        [Trait("Application", "UpdateCartItemQuantity - UseCase")]
        public async Task Execute_WhenItemNotFoundInCart_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var cart = _fixture.CreateCartWithItems(clientId);
            var nonExistentItemId = Guid.NewGuid();
            var input = _fixture.GetValidInput(nonExistentItemId, 5);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Item não encontrado no carrinho.");
            _fixture.CartRepositoryMock.Verify(r => r.Update(It.IsAny<Bcommerce.Domain.Sales.Carts.Cart>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }