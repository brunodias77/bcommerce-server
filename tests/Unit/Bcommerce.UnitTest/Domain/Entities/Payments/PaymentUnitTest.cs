using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Payments;
using Bcommerce.Domain.Sales.Payments.Enums;
using FluentAssertions;
using System;
using System.Threading;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Payments
{
    [Collection(nameof(PaymentTestFixture))]
    public class PaymentUnitTest
    {
        private readonly PaymentTestFixture _fixture;

        public PaymentUnitTest(PaymentTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Pagamento com Status Pendente")]
        [Trait("Domain", "Payment - Entity")]
        public void NewPayment_WithValidData_ShouldCreateWithPendingStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var amount = Money.Create(100m);
            var method = PaymentMethod.CreditCard;

            // Act
            var payment = Payment.NewPayment(orderId, amount, method);

            // Assert
            payment.Should().NotBeNull();
            payment.OrderId.Should().Be(orderId);
            payment.Amount.Should().Be(amount);
            payment.Method.Should().Be(method);
            payment.Status.Should().Be(PaymentStatus.Pending);
            payment.ProcessedAt.Should().BeNull();
        }

        [Fact(DisplayName = "Deve Marcar Pagamento Pendente como Aprovado")]
        [Trait("Domain", "Payment - Entity")]
        public void MarkAsApproved_WhenStatusIsPending_ShouldSetStatusToApproved()
        {
            // Arrange
            var payment = _fixture.CreateValidPendingPayment();
            var transactionId = Guid.NewGuid().ToString();

            // Act
            payment.MarkAsApproved(transactionId);

            // Assert
            payment.Status.Should().Be(PaymentStatus.Approved);
            payment.TransactionId.Should().Be(transactionId);
            payment.ProcessedAt.Should().NotBeNull();
        }

        [Fact(DisplayName = "Deve Marcar Pagamento Pendente como Recusado")]
        [Trait("Domain", "Payment - Entity")]
        public void MarkAsDeclined_WhenStatusIsPending_ShouldSetStatusToDeclined()
        {
            // Arrange
            var payment = _fixture.CreateValidPendingPayment();

            // Act
            payment.MarkAsDeclined();

            // Assert
            payment.Status.Should().Be(PaymentStatus.Declined);
            payment.ProcessedAt.Should().NotBeNull();
        }

        [Fact(DisplayName = "Deve Marcar Pagamento Aprovado como Reembolsado")]
        [Trait("Domain", "Payment - Entity")]
        public void MarkAsRefunded_WhenStatusIsApproved_ShouldSetStatusToRefunded()
        {
            // Arrange
            var payment = _fixture.CreateValidPendingPayment();
            payment.MarkAsApproved(Guid.NewGuid().ToString()); // Passo 1: Aprova o pagamento.
            var approvalDate = payment.ProcessedAt;
            Thread.Sleep(10); // Delay para garantir que o timestamp do reembolso seja diferente.

            // Act
            payment.MarkAsRefunded(); // Passo 2: Reembolsa o pagamento aprovado.

            // Assert
            payment.Status.Should().Be(PaymentStatus.Refunded);
            payment.ProcessedAt.Should().BeAfter(approvalDate.Value);
        }

        [Fact(DisplayName = "Não Deve Reembolsar Pagamento que não foi Aprovado")]
        [Trait("Domain", "Payment - Entity")]
        public void MarkAsRefunded_WhenStatusIsNotApproved_ShouldThrowException()
        {
            // Arrange
            var payment = _fixture.CreateValidPendingPayment(); // Status inicial é 'Pending'.

            // Act
            Action action = () => payment.MarkAsRefunded();

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("Somente pagamentos aprovados podem ser reembolsados.");
        }
    }
}