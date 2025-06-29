using Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation.Handlers;
using Bogus;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Catalog.Categories.ListCategories
{
    [CollectionDefinition(nameof(ListCategoriesUseCaseTestFixture))]
    public class ListCategoriesUseCaseTestFixtureCollection : ICollectionFixture<ListCategoriesUseCaseTestFixture> { }

    public class ListCategoriesUseCaseTestFixture
    {
        public Faker Faker { get; }
        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }

        public ListCategoriesUseCaseTestFixture()
        {
            Faker = new Faker("pt_BR");
            CategoryRepositoryMock = new Mock<ICategoryRepository>();
        }

        public ListCategoriesUseCase CreateUseCase()
        {
            return new ListCategoriesUseCase(CategoryRepositoryMock.Object);
        }

        public List<Category> CreateValidCategories(int count = 3)
        {
            // CORREÇÃO: Adicionando o parâmetro 'sortOrder' que faltava.
            return Enumerable.Range(1, count).Select(_ => Category.NewCategory(
                Faker.Commerce.Categories(1)[0],
                Faker.Lorem.Sentence(),
                null,
                Faker.Random.Int(0, 100), // sortOrder
                Notification.Create()
            )).ToList();
        }
        
        public ListCategoriesInput GetValidInput() => new();
    }
}