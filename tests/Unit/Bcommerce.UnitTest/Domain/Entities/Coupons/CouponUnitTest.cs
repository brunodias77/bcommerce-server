using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Marketing.Coupons;
using Bcommerce.Domain.Validation.Handlers;
using FluentAssertions;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Coupons
{
    [Collection(nameof(CouponTestFixture))]
    public class CouponUnitTest
    {
        private readonly CouponTestFixture _fixture;

        public CouponUnitTest(CouponTestFixture fixture)
        {
            _fixture = fixture;
        }

        // --- TESTES DE CRIAÇÃO E VALIDAÇÃO ---

        [Fact(DisplayName = "Deve Criar Cupom de Percentual Válido")]
        [Trait("Domain", "Coupon - Aggregate")]
        public void NewPercentageCoupon_WithValidData_ShouldCreateSuccessfully()
        {
            var handler = Notification.Create();
            var coupon = Coupon.NewPercentageCoupon(_fixture.GetValidCode(), 15, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), handler);
            
            handler.HasError().Should().BeFalse();
            coupon.Should().NotBeNull();
            coupon.DiscountPercentage.Should().Be(15);
            coupon.DiscountAmount.Should().BeNull();
        }
        
        [Fact(DisplayName = "Deve Criar Cupom de Valor Fixo Válido")]
        [Trait("Domain", "Coupon - Aggregate")]
        public void NewAmountCoupon_WithValidData_ShouldCreateSuccessfully()
        {
            var handler = Notification.Create();
            var amount = Money.Create(50);
            var coupon = Coupon.NewAmountCoupon(_fixture.GetValidCode(), amount, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), handler);
            
            handler.HasError().Should().BeFalse();
            coupon.Should().NotBeNull();
            coupon.DiscountAmount.Should().Be(amount);
            coupon.DiscountPercentage.Should().BeNull();
        }

        [Theory(DisplayName = "Não Deve Criar Cupom com Dados Inválidos")]
        [Trait("Domain", "Coupon - Aggregate")]
        [InlineData("", 10, "O código do cupom é obrigatório.")]
        [InlineData("VALID-CODE", 110, "A porcentagem de desconto deve ser entre 0 e 100.")]
        [InlineData("VALID-CODE", 0, "A porcentagem de desconto deve ser entre 0 e 100.")]
        public void NewCoupon_WithInvalidData_ShouldReturnError(string code, decimal percentage, string expectedError)
        {
            var handler = Notification.Create();
            Coupon.NewPercentageCoupon(code, percentage, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), handler);

            handler.HasError().Should().BeTrue();
            handler.FirstError()!.Message.Should().Be(expectedError);
        }
        
        // --- TESTES DO MÉTODO IsValid() ---

        [Fact(DisplayName = "Deve ser Válido para Cupom Ativo e Dentro das Regras")]
        [Trait("Domain", "Coupon - IsValid")]
        public void IsValid_WhenCouponIsActiveAndWithinRules_ShouldReturnTrue()
        {
            var coupon = _fixture.CreateValidPercentageCoupon();
            coupon.IsValid(Money.Create(100), null).Should().BeTrue();
        }

        [Fact(DisplayName = "Deve ser Inválido para Cupom Inativo")]
        [Trait("Domain", "Coupon - IsValid")]
        public void IsValid_WhenCouponIsInactive_ShouldReturnFalse()
        {
            var coupon = _fixture.CreateValidPercentageCoupon();
            coupon.Deactivate();
            coupon.IsValid(Money.Create(100), null).Should().BeFalse();
        }
        
        [Fact(DisplayName = "Deve ser Inválido para Cupom Expirado")]
        [Trait("Domain", "Coupon - IsValid")]
        public void IsValid_WhenExpired_ShouldReturnFalse()
        {
            var coupon = _fixture.CreateExpiredCoupon();
            coupon.IsValid(Money.Create(100), null).Should().BeFalse();
        }

        [Fact(DisplayName = "Deve ser Inválido se Limite de Usos foi Atingido")]
        [Trait("Domain", "Coupon - IsValid")]
        public void IsValid_WhenMaxUsesReached_ShouldReturnFalse()
        {
            var coupon = _fixture.CreateCouponWithUses(2, 2);
            coupon.IsValid(Money.Create(100), null).Should().BeFalse();
        }
        
        [Fact(DisplayName = "Deve ser Inválido para Usuário Incorreto")]
        [Trait("Domain", "Coupon - IsValid")]
        public void IsValid_WhenUserSpecificForWrongUser_ShouldReturnFalse()
        {
            var correctClientId = Guid.NewGuid();
            var wrongClientId = Guid.NewGuid();
            var coupon = _fixture.CreateUserSpecificCoupon(correctClientId);
            
            coupon.IsValid(Money.Create(100), wrongClientId).Should().BeFalse();
        }
        
        // --- TESTES DO MÉTODO Use() ---
        
        [Fact(DisplayName = "Deve Incrementar Usos ao Chamar Use()")]
        [Trait("Domain", "Coupon - Use")]
        public void Use_WhenCalled_ShouldIncrementTimesUsed()
        {
            var coupon = _fixture.CreateValidPercentageCoupon();
            var initialUses = coupon.TimesUsed;

            coupon.Use();

            coupon.TimesUsed.Should().Be(initialUses + 1);
        }
        
        [Fact(DisplayName = "Deve Lançar Exceção ao Usar Cupom Além do Limite")]
        [Trait("Domain", "Coupon - Use")]
        public void Use_WhenMaxUsesReached_ShouldThrowException()
        {
            var coupon = _fixture.CreateCouponWithUses(1, 1);
            Action action = () => coupon.Use();

            action.Should().Throw<DomainException>()
                .WithMessage("Este cupom já atingiu o limite máximo de usos.");
        }
    }
}