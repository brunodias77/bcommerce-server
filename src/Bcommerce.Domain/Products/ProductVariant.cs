using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Products;

public class ProductVariant : Entity
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; }
    public Guid? ColorId { get; private set; }
    public Guid? SizeId { get; private set; }
    public int StockQuantity { get; private set; }
    public decimal AdditionalPrice { get; private set; } // Preço adicional em relação ao produto base
    public bool IsActive { get; private set; }

    private ProductVariant() {}

    public static ProductVariant NewVariant(Guid productId, string sku, Guid? colorId, Guid? sizeId, int stock, decimal additionalPrice)
    {
        return new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            ColorId = colorId,
            SizeId = sizeId,
            StockQuantity = stock,
            AdditionalPrice = additionalPrice,
            IsActive = true
        };
    }
    
    public static ProductVariant With(
        Guid id, Guid productId, string sku, Guid? colorId, Guid? sizeId,
        int stockQuantity, decimal additionalPrice, bool isActive)
    {
        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            ColorId = colorId,
            SizeId = sizeId,
            StockQuantity = stockQuantity,
            AdditionalPrice = additionalPrice,
            IsActive = isActive
        };
        variant.Id = id;
        return variant;
    }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentException("Estoque não pode ser negativo.");
        StockQuantity = newQuantity;
    }

    public override void Validate(IValidationHandler handler) { /* Validações de SKU, estoque, etc. */ }
}