using Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using Bogus;
using Moq;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.UpdateCategory
{
    [CollectionDefinition(nameof(UpdateCategoryUseCaseTestFixture))]
    public class UpdateCategoryUseCaseTestFixtureCollection : ICollectionFixture<UpdateCategoryUseCaseTestFixture> { }

    public class UpdateCategoryUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public UpdateCategoryUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public UpdateCategoryUseCase CreateUseCase()
        {
            return new UpdateCategoryUseCase(CategoryRepositoryMock.Object, UnitOfWorkMock.Object);
        }

        public UpdateCategoryInput GetValidInput(Guid categoryId)
        {
            // CORREÇÃO: O input agora corresponde exatamente ao record definido na aplicação.
            return new UpdateCategoryInput(
                categoryId,
                Faker.Commerce.Categories(1)[0],
                Faker.Lorem.Sentence(),
                Faker.Random.Int(1, 100)
            );
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