using Bcomerce.Application.UseCases.Catalog.Categories.GetCategoryById;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.GetCategoryById
{
    [CollectionDefinition(nameof(GetCategoryByIdUseCaseTestFixture))]
    public class GetCategoryByIdUseCaseTestFixtureCollection : ICollectionFixture<GetCategoryByIdUseCaseTestFixture> { }

    public class GetCategoryByIdUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }

        public GetCategoryByIdUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
        }

        public GetCategoryByIdUseCase CreateUseCase()
        {
            return new GetCategoryByIdUseCase(CategoryRepositoryMock.Object);
        }

        public Category CreateValidCategory()
        {
            // CORREÇÃO: Adicionando o parâmetro 'sortOrder' que faltava.
            return Category.NewCategory(
                Faker.Commerce.Categories(1)[0],
                Faker.Lorem.Sentence(),
                null,
                Faker.Random.Int(0, 100), // sortOrder
                Notification.Create()
            );
        }
    }
}