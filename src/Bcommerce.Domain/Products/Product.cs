using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Products.Validators;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcommerce.Domain.Products;

public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public int StockQuantity { get; private set; } // Para produtos sem variantes
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }

    // Coleção de entidades filhas
    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() {}

    public static Product NewProduct(string name, string slug, decimal basePrice, int stock, Guid categoryId, Guid? brandId)
    {
        var product = new Product
        {
            Name = name,
            Slug = slug,
            BasePrice = basePrice,
            StockQuantity = stock,
            CategoryId = categoryId,
            BrandId = brandId,
            IsActive = true
        };
        product.Validate(Notification.Create());
        return product;
    }
    
    public static Product With(
        Guid id, string name, string slug, string? description, decimal basePrice,
        int stockQuantity, bool isActive, Guid categoryId, Guid? brandId)
    {
        var product = new Product
        {
            Name = name,
            Slug = slug,
            Description = description,
            BasePrice = basePrice,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            CategoryId = categoryId,
            BrandId = brandId
        };
        product.Id = id;
        return product;
    }

    public void AddVariant(ProductVariant variant)
    {
        // Regra de negócio: não adicionar variante com SKU duplicado
        if (_variants.Any(v => v.Sku == variant.Sku))
        {
            // Lançar exceção ou usar o handler de validação
            return;
        }
        _variants.Add(variant);
        // Ao adicionar uma variante, o estoque do produto base pode se tornar irrelevante
        this.StockQuantity = 0;
    }

    public override void Validate(IValidationHandler handler)
    {
        new ProductValidator(this, handler).Validate();

    }
}