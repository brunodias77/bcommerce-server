using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;

namespace Bcommerce.UnitTest.Domain.Entities.Categories;

public class CategoryTestFixture
{
    public Faker Faker { get; }

    public CategoryTestFixture()
    {
        Faker = new Faker("pt_BR");
    }

    public string GetValidCategoryName()
        => Faker.Commerce.Categories(1)[0];

    public string GetValidCategoryDescription()
        => Faker.Lorem.Sentence();

    public Category CreateValidCategory()
    {
        var category = Category.NewCategory(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            null,
            Faker.Random.Int(0, 100),
            Notification.Create()
        );
        category.ClearEvents(); // Limpa eventos para não interferir em outros testes
        return category;
    }
}

[CollectionDefinition(nameof(CategoryTestFixture))]
public class CategoryTestFixtureCollection : ICollectionFixture<CategoryTestFixture>
{
}