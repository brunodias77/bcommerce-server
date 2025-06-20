using Bcommerce.Domain.Catalog.Products.Entities;
using Bcommerce.Domain.Catalog.Products.Validators;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products;

public class Product : AggregateRoot
{
    // Propriedades Diretas
    public string BaseSku { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public Money BasePrice { get; private set; }
    public Money? SalePrice { get; private set; }
    public int StockQuantity { get; private set; } // Para produtos sem variação
    public bool IsActive { get; private set; }
    public Dimensions Dimensions { get; private set; }
    
    // Referências a outros agregados
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    
    // Coleções de Entidades Filhas
    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    
    // Adicione a coleção de variantes aqui se desejar
    // private readonly List<ProductVariant> _variants = new();
    // public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    
    private Product() : base() { }

    public static Product NewProduct(
        string baseSku, string name, string? description,
        Money basePrice, int stockQuantity, Guid categoryId, Guid? brandId,
        Dimensions dimensions, IValidationHandler handler)
    {
        var product = new Product
        {
            BaseSku = baseSku,
            Name = name,
            Description = description,
            BasePrice = basePrice,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            BrandId = brandId,
            Dimensions = dimensions,
            Slug = GenerateSlug(name),
            IsActive = true
        };
        
        product.Validate(handler);
        // Lógica de evento de criação aqui
        return product;
    }
    
    // <summary>
    /// Fábrica para reconstruir um produto existente a partir da persistência.
    /// Não executa validações de negócio ou dispara eventos de criação.
    /// </summary>
    public static Product With(
        Guid id, string baseSku, string name, string slug, string? description,
        Money basePrice, Money? salePrice, int stockQuantity, bool isActive,
        Dimensions dimensions, Guid categoryId, Guid? brandId,
        IEnumerable<ProductImage> images)
    {
        var product = new Product
        {
            BaseSku = baseSku,
            Name = name,
            Slug = slug,
            Description = description,
            BasePrice = basePrice,
            SalePrice = salePrice,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            Dimensions = dimensions,
            CategoryId = categoryId,
            BrandId = brandId
        };
        product.Id = id; // Atribui o ID existente
        
        // Adiciona as entidades filhas que foram carregadas do banco
        if (images != null)
        {
            product._images.AddRange(images);
        }

        return product;
    }
    
    
    public override void Validate(IValidationHandler handler)
    {
        new ProductValidator(this, handler).Validate();
    }
    
    // --- MÉTODOS DE NEGÓCIO ---

    public void AddImage(string imageUrl, string? altText)
    {
        // Regra de negócio: a primeira imagem adicionada é a capa.
        bool isCover = !_images.Any(); 
        int sortOrder = _images.Any() ? _images.Max(i => i.SortOrder) + 1 : 0;
        
        var newImage = ProductImage.NewImage(this.Id, imageUrl, altText, isCover, sortOrder);
        _images.Add(newImage);
    }
    
    public void SetCoverImage(Guid imageId)
    {
        var currentCover = _images.FirstOrDefault(i => i.IsCover);
        currentCover?.SetCover(false); // Desmarca a capa antiga

        var newCover = _images.FirstOrDefault(i => i.Id == imageId);
        DomainException.ThrowWhen(newCover is null, "Imagem não encontrada neste produto.");
        
        newCover.SetCover(true); // Marca a nova capa
    }

    public void ChangeBasePrice(Money newPrice)
    {
        DomainException.ThrowWhen(newPrice.Amount <= 0, "O preço base deve ser positivo.");
        BasePrice = newPrice;
        // RaiseEvent(new ProductPriceChangedEvent(this.Id, newPrice));
    }
    
    public void AdjustStock(int quantity)
    {
        DomainException.ThrowWhen(quantity < 0, "A quantidade em estoque não pode ser negativa.");
        StockQuantity = quantity;
        // RaiseEvent(new ProductStockUpdatedEvent(this.Id, quantity));
    }
    
    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "-");
    }
}