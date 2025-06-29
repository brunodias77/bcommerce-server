using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Orders;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Orders.CreateOrder;

[Collection(nameof(CreateOrderUseCaseTestFixture))]
    public class CreateOrderUseCaseTest
    {
        private readonly CreateOrderUseCaseTestFixture _fixture;

        public CreateOrderUseCaseTest(CreateOrderUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.ClientRepositoryMock.Invocations.Clear();
            _fixture.AddressRepositoryMock.Invocations.Clear();
            _fixture.CartRepositoryMock.Invocations.Clear();
            _fixture.OrderRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Criar Pedido com Sucesso a Partir do Carrinho")]
        [Trait("Application", "CreateOrder - UseCase")]
        public async Task Execute_WhenAllDataIsValid_ShouldCreateOrderAndClearCart()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var cart = _fixture.CreateCartWithItems(client.Id);
            var shippingAddress = _fixture.CreateValidAddress(client.Id);
            var billingAddress = _fixture.CreateValidAddress(client.Id);
            var input = _fixture.GetValidInput(shippingAddress.Id, billingAddress.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(client.Id);
            _fixture.ClientRepositoryMock.Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
            _fixture.AddressRepositoryMock.Setup(r => r.GetByIdAsync(shippingAddress.Id, It.IsAny<CancellationToken>())).ReturnsAsync(shippingAddress);
            _fixture.AddressRepositoryMock.Setup(r => r.GetByIdAsync(billingAddress.Id, It.IsAny<CancellationToken>())).ReturnsAsync(billingAddress);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Status.Should().Be(Bcommerce.Domain.Sales.Orders.Enums.OrderStatus.Pending);
            
            // Verifica se o pedido foi inserido e o carrinho atualizado (limpo)
            _fixture.OrderRepositoryMock.Verify(r => r.Insert(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.CartRepositoryMock.Verify(r => r.Update(It.Is<Cart>(c => c.Items.Count == 0), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Criar Pedido se Carrinho Estiver Vazio")]
        [Trait("Application", "CreateOrder - UseCase")]
        public async Task Execute_WhenCartIsEmpty_ShouldReturnError()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var emptyCart = Cart.NewCart(client.Id); // Carrinho sem itens
            var shippingAddress = _fixture.CreateValidAddress(client.Id);
            var input = _fixture.GetValidInput(shippingAddress.Id, shippingAddress.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(client.Id);
            _fixture.ClientRepositoryMock.Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(emptyCart);
            _fixture.AddressRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(shippingAddress);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Seu carrinho está vazio.");
            _fixture.OrderRepositoryMock.Verify(r => r.Insert(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Criar Pedido se Endereço de Entrega for Inválido")]
        [Trait("Application", "CreateOrder - UseCase")]
        public async Task Execute_WhenShippingAddressIsInvalid_ShouldReturnError()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var cart = _fixture.CreateCartWithItems(client.Id);
            var invalidAddressId = Guid.NewGuid();
            var input = _fixture.GetValidInput(invalidAddressId, Guid.NewGuid());
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(client.Id);
            _fixture.ClientRepositoryMock.Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
            _fixture.AddressRepositoryMock.Setup(r => r.GetByIdAsync(invalidAddressId, It.IsAny<CancellationToken>())).ReturnsAsync((Bcommerce.Domain.Customers.Clients.Entities.Address)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Endereço de entrega inválido.");
        }

        [Fact(DisplayName = "Deve Fazer Rollback se Ocorrer Erro na Persistência")]
        [Trait("Application", "CreateOrder - UseCase")]
        public async Task Execute_WhenRepositoryThrows_ShouldRollback()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var cart = _fixture.CreateCartWithItems(client.Id);
            var address = _fixture.CreateValidAddress(client.Id);
            var input = _fixture.GetValidInput(address.Id, address.Id);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(client.Id);
            _fixture.ClientRepositoryMock.Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
            _fixture.AddressRepositoryMock.Setup(r => r.GetByIdAsync(address.Id, It.IsAny<CancellationToken>())).ReturnsAsync(address);
            _fixture.OrderRepositoryMock.Setup(r => r.Insert(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database Error"));

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Não foi possível finalizar seu pedido. Tente novamente.");
            _fixture.UnitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }