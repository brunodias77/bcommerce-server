using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.Entities;

public class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; }
    public string? AltText { get; private set; }
    public bool IsCover { get; private set; }
    public int SortOrder { get; private set; }
    
    private ProductImage() { }
    
    // Este método de fábrica é chamado pelo Agregado Product
    public static ProductImage NewImage(
        Guid productId, string imageUrl, string? altText, bool isCover, int sortOrder)
    {
        // Validações simples aqui
        return new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            AltText = altText,
            IsCover = isCover,
            SortOrder = sortOrder
        };
    }

    internal void SetCover(bool isCover) => IsCover = isCover;

    public override void Validate(IValidationHandler handler)
    {
        // Implementar se houver regras complexas para uma imagem
    }
}