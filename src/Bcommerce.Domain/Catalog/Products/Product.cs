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
        public DateTime? SalePriceStartDate { get; private set; }
        public DateTime? SalePriceEndDate { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; }
        public Dimensions Dimensions { get; private set; }
        public DateTime CreatedAt { get; private set; } // ADICIONADO
        public DateTime UpdatedAt { get; private set; } // ADICIONADO

        // Referências a outros agregados
        public Guid CategoryId { get; private set; }
        public Guid? BrandId { get; private set; }

        // Coleções de Entidades Filhas
        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        private readonly List<ProductVariant> _variants = new();
        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

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
                IsActive = true,
                CreatedAt = DateTime.UtcNow, // ADICIONADO
                UpdatedAt = DateTime.UtcNow  // ADICIONADO
            };

            product.Validate(handler);
            return product;
        }
        
        public static Product With(
            Guid id, string baseSku, string name, string slug, string? description,
            Money basePrice, Money? salePrice, DateTime? salePriceStartDate, DateTime? salePriceEndDate, 
            int stockQuantity, bool isActive, Dimensions dimensions, Guid categoryId, Guid? brandId,
            DateTime createdAt, DateTime updatedAt, // ADICIONADO
            IEnumerable<ProductImage>? images, IEnumerable<ProductVariant>? variants)
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
                Dimensions = dimensions,
                CategoryId = categoryId,
                BrandId = brandId,
                CreatedAt = createdAt, // ADICIONADO
                UpdatedAt = updatedAt  // ADICIONADO
            };

            if (images != null) product._images.AddRange(images);
            if (variants != null) product._variants.AddRange(variants);

            return product;
        }

        public override void Validate(IValidationHandler handler)
        {
            new ProductValidator(this, handler).Validate();
        }

        // --- MÉTODOS DE NEGÓCIO ---
        
        public void Update(
            string name, string? description, Money newBasePrice, int newStockQuantity,
            bool isActive, Guid categoryId, Guid? brandId, Dimensions newDimensions,
            IValidationHandler handler)
        {
            Name = name;
            Description = description;
            BasePrice = newBasePrice;
            StockQuantity = newStockQuantity;
            IsActive = isActive;
            CategoryId = categoryId;
            BrandId = brandId;
            Dimensions = newDimensions;
            UpdatedAt = DateTime.UtcNow;
    
            // Gera um novo slug caso o nome mude
            Slug = GenerateSlug(name);
    
            // Revalida o estado completo da entidade
            Validate(handler);
        }

        public void AddImage(string imageUrl, string? altText)
        {
            bool isCover = !_images.Any();
            int sortOrder = _images.Any() ? _images.Max(i => i.SortOrder) + 1 : 0;
            
            var newImage = ProductImage.NewImage(Id, imageUrl, altText, isCover, sortOrder);
            _images.Add(newImage);
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void AddVariant(ProductVariant variant)
        {
            var combinationExists = _variants.Any(v => v.ColorId == variant.ColorId && v.SizeId == variant.SizeId);
            DomainException.ThrowWhen(combinationExists, "Uma variação com estes atributos já existe.");
            
            var skuExists = _variants.Any(v => v.Sku.Equals(variant.Sku, StringComparison.OrdinalIgnoreCase));
            DomainException.ThrowWhen(skuExists, $"O SKU '{variant.Sku}' já está em uso por outra variante neste produto.");

            _variants.Add(variant);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeBasePrice(Money newPrice)
        {
            DomainException.ThrowWhen(newPrice.Amount <= 0, "O preço base deve ser positivo.");
            BasePrice = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSalePrice(Money salePrice, DateTime startDate, DateTime endDate)
        {
            DomainException.ThrowWhen(salePrice.Amount >= BasePrice.Amount, "Preço de oferta deve ser menor que o preço base.");
            DomainException.ThrowWhen(startDate >= endDate, "Data de início da oferta deve ser anterior à data de término.");
            
            SalePrice = salePrice;
            SalePriceStartDate = startDate;
            SalePriceEndDate = endDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveSalePrice()
        {
            SalePrice = null;
            SalePriceStartDate = null;
            SalePriceEndDate = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AdjustStock(int quantity)
        {
            DomainException.ThrowWhen(quantity < 0, "A quantidade em estoque não pode ser negativa.");
            StockQuantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string GenerateSlug(string text)
        {
            return text.ToLowerInvariant().Replace(" ", "-");
        }
    }







