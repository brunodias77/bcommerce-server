using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Brands
{
    [CollectionDefinition(nameof(BrandTestFixture))]
    public class BrandTestFixtureCollection : ICollectionFixture<BrandTestFixture> { }

    public class BrandTestFixture
    {
        public Faker Faker { get; }

        public BrandTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public (string Name, string Description, string? LogoUrl) GetValidBrandInputData()
        {
            // Usando Company.CompanyName para nomes de marcas mais realistas
            return (
                Faker.Company.CompanyName(),
                Faker.Lorem.Sentence(),
                Faker.Internet.Url()
            );
        }

        public Brand CreateValidBrand()
        {
            var (name, description, logoUrl) = GetValidBrandInputData();
            var brand = Brand.NewBrand(
                name,
                description,
                logoUrl,
                Notification.Create()
            );
            // Limpa o evento de criação para não interferir nos testes seguintes
            brand.ClearEvents();
            return brand;
        }
    }
}