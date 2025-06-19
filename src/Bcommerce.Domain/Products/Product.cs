using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Products.Validators;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcommerce.Domain.Products;

public class Product : AggregateRoot
{
    public string BaseSku { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal? SalePrice { get; private set; }
    public DateTime? SalePriceStartDate { get; private set; }
    public DateTime? SalePriceEndDate { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public decimal? WeightKg { get; private set; }
    public int? HeightCm { get; private set; }
    public int? WidthCm { get; private set; }
    public int? DepthCm { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() 
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Product NewProduct(
        string baseSku, string name, string slug, decimal basePrice, 
        int stock, Guid categoryId, Guid? brandId)
    {
        var product = new Product
        {
            BaseSku = baseSku,
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
        Guid id, string baseSku, string name, string slug, string? description, 
        decimal basePrice, decimal? salePrice, DateTime? salePriceStartDate, 
        DateTime? salePriceEndDate, int stockQuantity, bool isActive, 
        decimal? weightKg, int? heightCm, int? widthCm, int? depthCm,
        Guid categoryId, Guid? brandId, DateTime createdAt, 
        DateTime updatedAt, DateTime? deletedAt)
    {
        var product = new Product
        {
            Id = id,
            BaseSku = baseSku,
            Name = name,
            Slug = slug,
            Description = description,
            BasePrice = basePrice,
            SalePrice = salePrice,
            SalePriceStartDate = salePriceStartDate,
            SalePriceEndDate = salePriceEndDate,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            WeightKg = weightKg,
            HeightCm = heightCm,
            WidthCm = widthCm,
            DepthCm = depthCm,
            CategoryId = categoryId,
            BrandId = brandId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
        };
        return product;
    }

    public void SetSalePrice(decimal salePrice, DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }
        
        SalePrice = salePrice;
        SalePriceStartDate = startDate;
        SalePriceEndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSalePrice()
    {
        SalePrice = null;
        SalePriceStartDate = null;
        SalePriceEndDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddVariant(ProductVariant variant)
    {
        if (_variants.Any(v => v.Sku == variant.Sku))
        {
            return;
        }
        _variants.Add(variant);
        this.StockQuantity = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public override void Validate(IValidationHandler handler)
    {
        new ProductValidator(this, handler).Validate();
    }
}

// using Bcommerce.Domain.Abstractions;
// using Bcommerce.Domain.Products.Validators;
// using Bcommerce.Domain.Validations;
// using Bcommerce.Domain.Validations.Handlers;
//
// namespace Bcommerce.Domain.Products;
//
// public class Product : AggregateRoot
// {
//     public string Name { get; private set; }
//     public string Slug { get; private set; }
//     public string? Description { get; private set; }
//     public decimal BasePrice { get; private set; }
//     public int StockQuantity { get; private set; } // Para produtos sem variantes
//     public bool IsActive { get; private set; }
//     public Guid CategoryId { get; private set; }
//     public Guid? BrandId { get; private set; }
//
//     // Coleção de entidades filhas
//     private readonly List<ProductVariant> _variants = new();
//     public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
//
//     private Product() {}
//
//     public static Product NewProduct(string name, string slug, decimal basePrice, int stock, Guid categoryId, Guid? brandId)
//     {
//         var product = new Product
//         {
//             Name = name,
//             Slug = slug,
//             BasePrice = basePrice,
//             StockQuantity = stock,
//             CategoryId = categoryId,
//             BrandId = brandId,
//             IsActive = true
//         };
//         product.Validate(Notification.Create());
//         return product;
//     }
//     
//     public static Product With(
//         Guid id, string name, string slug, string? description, decimal basePrice,
//         int stockQuantity, bool isActive, Guid categoryId, Guid? brandId)
//     {
//         var product = new Product
//         {
//             Name = name,
//             Slug = slug,
//             Description = description,
//             BasePrice = basePrice,
//             StockQuantity = stockQuantity,
//             IsActive = isActive,
//             CategoryId = categoryId,
//             BrandId = brandId
//         };
//         product.Id = id;
//         return product;
//     }
//
//     public void AddVariant(ProductVariant variant)
//     {
//         // Regra de negócio: não adicionar variante com SKU duplicado
//         if (_variants.Any(v => v.Sku == variant.Sku))
//         {
//             // Lançar exceção ou usar o handler de validação
//             return;
//         }
//         _variants.Add(variant);
//         // Ao adicionar uma variante, o estoque do produto base pode se tornar irrelevante
//         this.StockQuantity = 0;
//     }
//
//     public override void Validate(IValidationHandler handler)
//     {
//         new ProductValidator(this, handler).Validate();
//
//     }
// }