using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Enums;
using Bcommerce.Domain.Services;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Orders.ProcessPayment
{
    [Collection(nameof(ProcessPaymentUseCaseTestFixture))]
    public class ProcessPaymentUseCaseTest
    {
        private readonly ProcessPaymentUseCaseTestFixture _fixture;

        public ProcessPaymentUseCaseTest(ProcessPaymentUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.OrderRepositoryMock.Invocations.Clear();
            _fixture.PaymentGatewayMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Processar Pagamento com Sucesso")]
        [Trait("Application", "ProcessPayment - UseCase")]
        public async Task Execute_WhenGatewayApproves_ShouldUpdateOrderStatusAndCommit()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var order = _fixture.CreateValidPendingOrder(clientId);
            var input = _fixture.GetValidInput(order.Id, "approved-token");
            var useCase = _fixture.CreateUseCase();
            var gatewayResult = new PaymentGatewayResult(true, $"txn_{Guid.NewGuid()}", null);

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.OrderRepositoryMock.Setup(r => r.Get(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _fixture.PaymentGatewayMock.Setup(g => g.ProcessPaymentAsync(order, input.PaymentMethodToken, It.IsAny<CancellationToken>())).ReturnsAsync(gatewayResult);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Status.Should().Be(OrderStatus.Processing);

            _fixture.OrderRepositoryMock.Verify(r => r.Update(
                It.Is<Order>(o => o.Status == OrderStatus.Processing && o.Payments.First().Status == Bcommerce.Domain.Sales.Payments.Enums.PaymentStatus.Approved),
                It.IsAny<CancellationToken>()),
                Times.Once
            );
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Processar Pagamento se Gateway Recusar")]
        [Trait("Application", "ProcessPayment - UseCase")]
        public async Task Execute_WhenGatewayDeclines_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var order = _fixture.CreateValidPendingOrder(clientId);
            var input = _fixture.GetValidInput(order.Id, "declined-token");
            var useCase = _fixture.CreateUseCase();
            var gatewayResult = new PaymentGatewayResult(false, null, "Fundos insuficientes.");

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.OrderRepositoryMock.Setup(r => r.Get(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _fixture.PaymentGatewayMock.Setup(g => g.ProcessPaymentAsync(order, input.PaymentMethodToken, It.IsAny<CancellationToken>())).ReturnsAsync(gatewayResult);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Fundos insuficientes.");

            _fixture.OrderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Processar Pagamento se Pedido Não For Encontrado")]
        [Trait("Application", "ProcessPayment - UseCase")]
        public async Task Execute_WhenOrderIsNotFound_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidInput(Guid.NewGuid(), "any-token");
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(Guid.NewGuid());
            _fixture.OrderRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Pedido não encontrado.");
            _fixture.PaymentGatewayMock.Verify(g => g.ProcessPaymentAsync(It.IsAny<Order>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}