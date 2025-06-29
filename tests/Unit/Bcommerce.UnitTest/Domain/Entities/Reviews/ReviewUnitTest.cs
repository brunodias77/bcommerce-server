using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Reviews;
using FluentAssertions;
using System;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Reviews
{
    [Collection(nameof(ReviewTestFixture))]
    public class ReviewUnitTest
    {
        private readonly ReviewTestFixture _fixture;

        public ReviewUnitTest(ReviewTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Avaliação Válida e Iniciar como Não Aprovada")]
        [Trait("Domain", "Review - Entity")]
        public void NewReview_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var (clientId, productId, orderId, rating, comment) = _fixture.GetValidReviewInputData();

            // Act
            var review = Review.NewReview(clientId, productId, orderId, rating, comment);

            // Assert
            review.Should().NotBeNull();
            review.ProductId.Should().Be(productId);
            review.Rating.Should().Be(rating);
            review.Comment.Should().Be(comment);
            review.IsApproved.Should().BeFalse(); // Importante: toda avaliação deve começar como não aprovada.
        }

        [Theory(DisplayName = "Não Deve Criar Avaliação com Nota Inválida e Deve Lançar Exceção")]
        [Trait("Domain", "Review - Entity")]
        [InlineData(0)]
        [InlineData(6)]
        public void NewReview_WithInvalidRating_ShouldThrowDomainException(short invalidRating)
        {
            // Arrange
            var (clientId, productId, orderId, _, comment) = _fixture.GetValidReviewInputData();

            // Act
            Action action = () => Review.NewReview(clientId, productId, orderId, invalidRating, comment);

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("A avaliação deve ser entre 1 e 5 estrelas.");
        }

        [Fact(DisplayName = "Deve Aprovar uma Avaliação Não Aprovada")]
        [Trait("Domain", "Review - Entity")]
        public void Approve_WhenReviewIsNotApproved_ShouldSetIsApprovedToTrue()
        {
            // Arrange
            var review = _fixture.CreateValidReview();
            review.IsApproved.Should().BeFalse();

            // Act
            review.Approve();

            // Assert
            review.IsApproved.Should().BeTrue();
        }

        [Fact(DisplayName = "Deve Rejeitar uma Avaliação Aprovada")]
        [Trait("Domain", "Review - Entity")]
        public void Reject_WhenReviewIsApproved_ShouldSetIsApprovedToFalse()
        {
            // Arrange
            var review = _fixture.CreateValidReview();
            review.Approve(); // Primeiro, aprova a avaliação
            review.IsApproved.Should().BeTrue();

            // Act
            review.Reject();

            // Assert
            review.IsApproved.Should().BeFalse();
        }
    }
}