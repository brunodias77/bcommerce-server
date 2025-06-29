using Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Categories
{
    [CollectionDefinition(nameof(CreateCategoryUseCaseTestFixture))]
    public class CreateCategoryUseCaseTestFixtureCollection : ICollectionFixture<CreateCategoryUseCaseTestFixture> { }

    public class CreateCategoryUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public CreateCategoryUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public CreateCategoryInput GetValidInput()
        {
            return new CreateCategoryInput(
                Faker.Commerce.Categories(1)[0],
                Faker.Commerce.ProductDescription(),
                null,
                Faker.Random.Int(0, 100)
            );
        }

        public CreateCategoryUseCase CreateUseCase()
        {
            return new CreateCategoryUseCase(
                CategoryRepositoryMock.Object,
                UnitOfWorkMock.Object
            );
        }
    }
}