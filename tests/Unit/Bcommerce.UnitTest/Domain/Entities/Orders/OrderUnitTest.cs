using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Enums;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Orders
{
    [Collection(nameof(OrderTestFixture))]
    public class OrderUnitTest
    {
        private readonly OrderTestFixture _fixture;

        public OrderUnitTest(OrderTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Pedido Válido a Partir do Carrinho")]
        [Trait("Domain", "Order - Aggregate")]
        public void NewOrderFromCart_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var shippingAddress = _fixture.CreateValidAddress(client.Id);
            var billingAddress = _fixture.CreateValidAddress(client.Id, Bcommerce.Domain.Customers.Clients.Enums.AddressType.Billing);
            var cartItems = _fixture.CreateValidCartItems(Guid.NewGuid());
            var shippingAmount = _fixture.Faker.Random.Decimal(10, 25);
            var expectedItemsTotal = cartItems.Sum(item => item.GetTotal().Amount);
            var expectedGrandTotal = expectedItemsTotal + shippingAmount;
            
            // Act
            var order = Order.NewOrderFromCart(client, cartItems, Bcommerce.Domain.Catalog.Products.ValueObjects.Money.Create(shippingAmount), shippingAddress, billingAddress);

            // Assert
            order.Should().NotBeNull();
            order.ClientId.Should().Be(client.Id);
            order.Status.Should().Be(OrderStatus.Pending);
            order.Items.Should().HaveSameCount(cartItems);
            order.ItemsTotalAmount.Amount.Should().Be(expectedItemsTotal);
            order.GrandTotalAmount.Amount.Should().Be(expectedGrandTotal);
            order.ShippingAddress.Should().NotBeNull();
            order.BillingAddress.Should().NotBeNull();
            order.ShippingAddress.Street.Should().Be(shippingAddress.Street);
        }

        [Fact(DisplayName = "Deve Transitar Status de Pending para Processing")]
        [Trait("Domain", "Order - Aggregate")]
        public void SetAsProcessing_WhenStatusIsPending_ShouldChangeStatus()
        {
            // Arrange
            var order = _fixture.CreateValidOrder();

            // Act
            order.SetAsProcessing();

            // Assert
            order.Status.Should().Be(OrderStatus.Processing);
        }

        [Fact(DisplayName = "Não Deve Transitar para Processing se Status não for Pending")]
        [Trait("Domain", "Order - Aggregate")]
        public void SetAsProcessing_WhenStatusIsNotPending_ShouldThrowException()
        {
            // Arrange
            var order = _fixture.CreateValidOrder();
            order.SetAsProcessing();
            order.Ship(); // Status agora é Shipped

            // Act
            Action action = () => order.SetAsProcessing();

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("Somente pedidos pendentes podem ser movidos para processamento.");
        }

        [Fact(DisplayName = "Deve Cancelar um Pedido em Processamento")]
        [Trait("Domain", "Order - Aggregate")]
        public void Cancel_WhenOrderIsProcessing_ShouldChangeStatusToCanceled()
        {
            // Arrange
            var order = _fixture.CreateValidOrder();
            order.SetAsProcessing();

            // Act
            order.Cancel();

            // Assert
            order.Status.Should().Be(OrderStatus.Canceled);
        }

        [Fact(DisplayName = "Não Deve Cancelar um Pedido Já Entregue")]
        [Trait("Domain", "Order - Aggregate")]
        public void Cancel_WhenOrderIsDelivered_ShouldThrowException()
        {
            // Arrange
            var order = _fixture.CreateValidOrder();
            order.SetAsProcessing();
            order.Ship();
            order.Deliver(); // Status agora é Delivered

            // Act
            Action action = () => order.Cancel();

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("Não é possível cancelar um pedido com status 'Delivered'.");
        }
    }
}