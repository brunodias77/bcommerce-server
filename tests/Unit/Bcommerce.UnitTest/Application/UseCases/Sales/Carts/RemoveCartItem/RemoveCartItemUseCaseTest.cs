using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.RemoveCartItem;

[Collection(nameof(RemoveCartItemUseCaseTestFixture))]
public class RemoveCartItemUseCaseTest
{
    private readonly RemoveCartItemUseCaseTestFixture _fixture;

    public RemoveCartItemUseCaseTest(RemoveCartItemUseCaseTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.LoggedUserMock.Invocations.Clear();
        _fixture.CartRepositoryMock.Invocations.Clear();
        _fixture.UnitOfWorkMock.Invocations.Clear();
    }

    [Fact(DisplayName = "Deve Remover Item do Carrinho com Sucesso")]
    [Trait("Application", "RemoveCartItem - UseCase")]
    public async Task Execute_WhenItemExists_ShouldRemoveItemAndCommit()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var cart = _fixture.CreateCartWithItems(clientId, 2);
        var itemToRemove = cart.Items.First();
        var useCase = _fixture.CreateUseCase();

        _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
        _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        // Act
        var result = await useCase.Execute(itemToRemove.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Should().NotContain(i => i.CartItemId == itemToRemove.Id);
        _fixture.CartRepositoryMock.Verify(r => r.Update(cart, It.IsAny<CancellationToken>()), Times.Once);
        _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }
}