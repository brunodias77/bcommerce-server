using Bcommerce.Domain.Reviews;
using Bogus;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Reviews
{
    [CollectionDefinition(nameof(ReviewTestFixture))]
    public class ReviewTestFixtureCollection : ICollectionFixture<ReviewTestFixture> { }

    public class ReviewTestFixture
    {
        public Faker Faker { get; }

        public ReviewTestFixture()
        {
            Faker = new Faker("pt_BR");
        }

        public (Guid ClientId, Guid ProductId, Guid OrderId, short Rating, string? Comment) GetValidReviewInputData()
        {
            return (
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Faker.Random.Short(1, 5),
                Faker.Lorem.Sentence()
            );
        }

        public Review CreateValidReview()
        {
            var (clientId, productId, orderId, rating, comment) = GetValidReviewInputData();
            
            // CORREÇÃO: Chamando o método de fábrica correto, sem o handler.
            return Review.NewReview(
                clientId,
                productId,
                orderId,
                rating,
                comment
            );
        }
    }
}