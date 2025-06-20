using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Reviews;

public class Review : AggregateRoot
{
    public Guid ClientId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid OrderId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Review() { }

    public static Review NewReview(Guid clientId, Guid productId, Guid orderId, int rating, string? comment)
    {
        DomainException.ThrowWhen(rating < 1 || rating > 5, "A avaliação deve ser entre 1 e 5 estrelas.");
            
        return new Review
        {
            ClientId = clientId,
            ProductId = productId,
            OrderId = orderId,
            Rating = rating,
            Comment = comment,
            IsApproved = false, // Avaliações começam como não aprovadas por padrão
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve() => IsApproved = true;
    public void Reject() => IsApproved = false;

    public override void Validate(IValidationHandler handler)
    {
        // Validações se necessário
    }


}