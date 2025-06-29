using Bcommerce.Domain.Customers.Consents;
using Bcommerce.Domain.Customers.Consents.Enums;
using FluentAssertions;
using System;
using System.Threading;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Consents
{
    [Collection(nameof(ConsentTestFixture))]
    public class ConsentUnitTest
    {
        private readonly ConsentTestFixture _fixture;

        public ConsentUnitTest(ConsentTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Consentimento com Sucesso")]
        [Trait("Domain", "Consent - Entity")]
        public void NewConsent_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var type = ConsentType.MarketingEmail;
            var isGranted = true;
            var termsVersion = "1.0.0";

            // Act
            var consent = Consent.NewConsent(clientId, type, isGranted, termsVersion);

            // Assert
            consent.Should().NotBeNull();
            consent.ClientId.Should().Be(clientId);
            consent.Type.Should().Be(type);
            consent.IsGranted.Should().Be(isGranted);
            consent.TermsVersion.Should().Be(termsVersion);
        }

        [Fact(DisplayName = "Deve Conceder um Consentimento que estava Revogado")]
        [Trait("Domain", "Consent - Entity")]
        public void Grant_WhenConsentIsRevoked_ShouldSetIsGrantedToTrue()
        {
            // Arrange
            var consent = _fixture.CreateConsent(isGranted: false);
            var lastUpdate = consent.UpdatedAt;
            var newTermsVersion = "2.0.0";
            Thread.Sleep(10); // Pequeno delay para garantir que o timestamp mude

            // Act
            consent.Grant(newTermsVersion);

            // Assert
            consent.IsGranted.Should().BeTrue();
            consent.TermsVersion.Should().Be(newTermsVersion);
            consent.UpdatedAt.Should().BeAfter(lastUpdate);
        }

        [Fact(DisplayName = "Deve Ser Idempotente ao Conceder Consentimento Já Concedido")]
        [Trait("Domain", "Consent - Entity")]
        public void Grant_WhenConsentIsAlreadyGranted_ShouldDoNothing()
        {
            // Arrange
            var consent = _fixture.CreateConsent(isGranted: true);
            var lastUpdate = consent.UpdatedAt;

            // Act
            consent.Grant("2.0.0"); // Tenta conceder novamente

            // Assert
            consent.IsGranted.Should().BeTrue();
            consent.UpdatedAt.Should().Be(lastUpdate); // Data não deve mudar
        }

        [Fact(DisplayName = "Deve Revogar um Consentimento que estava Concedido")]
        [Trait("Domain", "Consent - Entity")]
        public void Revoke_WhenConsentIsGranted_ShouldSetIsGrantedToFalse()
        {
            // Arrange
            var consent = _fixture.CreateConsent(isGranted: true);
            var lastUpdate = consent.UpdatedAt;
            Thread.Sleep(10); // Pequeno delay

            // Act
            consent.Revoke();

            // Assert
            consent.IsGranted.Should().BeFalse();
            consent.UpdatedAt.Should().BeAfter(lastUpdate);
        }

        [Fact(DisplayName = "Deve Ser Idempotente ao Revogar Consentimento Já Revogado")]
        [Trait("Domain", "Consent - Entity")]
        public void Revoke_WhenConsentIsAlreadyRevoked_ShouldDoNothing()
        {
            // Arrange
            var consent = _fixture.CreateConsent(isGranted: false);
            var lastUpdate = consent.UpdatedAt;

            // Act
            consent.Revoke(); // Tenta revogar novamente

            // Assert
            consent.IsGranted.Should().BeFalse();
            consent.UpdatedAt.Should().Be(lastUpdate); // Data não deve mudar
        }
    }
}