using Bcomerce.Application.UseCases.Catalog.Categories.DeleteCategory;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.DeleteCategory
{
    [CollectionDefinition(nameof(DeleteCategoryUseCaseTestFixture))]
    public class DeleteCategoryUseCaseTestFixtureCollection : ICollectionFixture<DeleteCategoryUseCaseTestFixture> { }

    public class DeleteCategoryUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public DeleteCategoryUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public DeleteCategoryUseCase CreateUseCase()
        {
            return new DeleteCategoryUseCase(CategoryRepositoryMock.Object, UnitOfWorkMock.Object);
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