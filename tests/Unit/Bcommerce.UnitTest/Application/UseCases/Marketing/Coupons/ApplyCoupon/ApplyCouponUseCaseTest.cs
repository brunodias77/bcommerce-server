using Bcommerce.Domain.Marketing.Coupons;
using Bcommerce.Domain.Sales.Orders;
using FluentAssertions;
using Moq;

namespace Bcommerce.UnitTest.Application.UseCases.Marketing.Coupons.ApplyCoupon;

[Collection(nameof(ApplyCouponUseCaseTestFixture))]
    public class ApplyCouponUseCaseTest
    {
        private readonly ApplyCouponUseCaseTestFixture _fixture;

        public ApplyCouponUseCaseTest(ApplyCouponUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            // Limpa o histórico de chamadas antes de cada teste
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.OrderRepositoryMock.Invocations.Clear();
            _fixture.CouponRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Aplicar Cupom Válido a Pedido Pendente com Sucesso")]
        [Trait("Application", "ApplyCoupon - UseCase")]
        public async Task Execute_WhenCouponAndOrderAreValid_ShouldApplyDiscountAndCommit()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var order = _fixture.CreateValidPendingOrder(clientId);
            var coupon = _fixture.CreateValidCoupon();
            var input = _fixture.GetValidInput(order.Id, coupon.Code);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.OrderRepositoryMock.Setup(r => r.Get(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _fixture.CouponRepositoryMock.Setup(r => r.GetByCodeAsync(coupon.Code, It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TotalAmount.Should().BeLessThan(220); // 200 (itens) + 20 (frete) - 20 (10% de desconto) = 200
            
            _fixture.OrderRepositoryMock.Verify(r => r.Update(It.Is<Order>(o => o.DiscountAmount.Amount > 0), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.CouponRepositoryMock.Verify(r => r.Update(It.Is<Coupon>(c => c.TimesUsed == 1), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Aplicar Cupom se Pedido Não For Encontrado")]
        [Trait("Application", "ApplyCoupon - UseCase")]
        public async Task Execute_WhenOrderNotFound_ShouldReturnError()
        {
            // Arrange
            var input = _fixture.GetValidInput(Guid.NewGuid(), "ANY_CODE");
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(Guid.NewGuid());
            _fixture.OrderRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Pedido não encontrado.");
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Aplicar Cupom se Código For Inválido")]
        [Trait("Application", "ApplyCoupon - UseCase")]
        public async Task Execute_WhenCouponCodeIsInvalid_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var order = _fixture.CreateValidPendingOrder(clientId);
            var input = _fixture.GetValidInput(order.Id, "CODIGO-INVALIDO");
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.OrderRepositoryMock.Setup(r => r.Get(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _fixture.CouponRepositoryMock.Setup(r => r.GetByCodeAsync(input.CouponCode, It.IsAny<CancellationToken>())).ReturnsAsync((Coupon)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Cupom inválido.");
        }

        [Fact(DisplayName = "Deve Retornar Erro de Domínio se Regra de Negócio For Violada")]
        [Trait("Application", "ApplyCoupon - UseCase")]
        public async Task Execute_WhenDomainRuleIsViolated_ShouldReturnDomainError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var order = _fixture.CreateValidPendingOrder(clientId);
            order.SetAsProcessing(); // Pedido não está mais pendente, o que viola a regra do ApplyCoupon.
            var coupon = _fixture.CreateValidCoupon();
            var input = _fixture.GetValidInput(order.Id, coupon.Code);
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.OrderRepositoryMock.Setup(r => r.Get(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _fixture.CouponRepositoryMock.Setup(r => r.GetByCodeAsync(coupon.Code, It.IsAny<CancellationToken>())).ReturnsAsync(coupon);
            
            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.GetErrors().Should().Contain(e => e.Message == "Cupons só podem ser aplicados a pedidos pendentes.");
            _fixture.UnitOfWorkMock.Verify(uow => uow.Commit(), Times.Never);
        }
    }