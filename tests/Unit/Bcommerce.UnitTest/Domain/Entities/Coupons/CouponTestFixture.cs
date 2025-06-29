using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Marketing.Coupons;
using Bcommerce.Domain.Marketing.Coupons.Enums;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Coupons
{
    [CollectionDefinition(nameof(CouponTestFixture))]
    public class CouponTestFixtureCollection : ICollectionFixture<CouponTestFixture> { }

    public class CouponTestFixture
    {
        public Faker Faker { get; }

        public CouponTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public string GetValidCode() => Faker.Commerce.Ean8().ToUpper();

        // --- MÉTODOS PARA CRIAR CUPONS EM DIFERENTES ESTADOS ---

        public Coupon CreateValidPercentageCoupon(int? maxUses = 100)
        {
            return Coupon.NewPercentageCoupon(
                code: GetValidCode(),
                percentage: Faker.Random.Decimal(5, 50),
                validFrom: DateTime.UtcNow.AddDays(-1),
                validUntil: DateTime.UtcNow.AddDays(7),
                handler: Notification.Create(),
                maxUses: maxUses
            );
        }
        
        public Coupon CreateCouponWithUses(int uses, int? maxUses)
        {
             var coupon = Coupon.NewPercentageCoupon(
                code: GetValidCode(),
                percentage: 10,
                validFrom: DateTime.UtcNow.AddDays(-1),
                validUntil: DateTime.UtcNow.AddDays(7),
                handler: Notification.Create(),
                maxUses: maxUses
            );
            for(int i = 0; i < uses; i++) coupon.Use();
            return coupon;
        }

        public Coupon CreateExpiredCoupon()
        {
            return Coupon.NewPercentageCoupon(
                code: GetValidCode(),
                percentage: 10,
                validFrom: DateTime.UtcNow.AddDays(-10),
                validUntil: DateTime.UtcNow.AddDays(-1),
                handler: Notification.Create()
            );
        }
        
        public Coupon CreateUserSpecificCoupon(Guid clientId)
        {
            return Coupon.NewPercentageCoupon(
                code: GetValidCode(),
                percentage: 20,
                validFrom: DateTime.UtcNow.AddDays(-1),
                validUntil: DateTime.UtcNow.AddDays(7),
                handler: Notification.Create(),
                clientId: clientId
            );
        }
    }
}