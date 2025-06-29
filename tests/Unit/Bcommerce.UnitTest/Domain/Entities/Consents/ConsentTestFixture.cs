using Bcommerce.Domain.Customers.Consents;
using Bcommerce.Domain.Customers.Consents.Enums;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Consents
{
    [CollectionDefinition(nameof(ConsentTestFixture))]
    public class ConsentTestFixtureCollection : ICollectionFixture<ConsentTestFixture> { }

    public class ConsentTestFixture
    {
        public Faker Faker { get; }

        public ConsentTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public Consent CreateConsent(
            bool isGranted, 
            ConsentType type = ConsentType.MarketingEmail, 
            Guid? clientId = null)
        {
            return Consent.NewConsent(
                clientId ?? Guid.NewGuid(), 
                type, 
                isGranted, 
                Faker.System.Version().ToString()
            );
        }
    }
}