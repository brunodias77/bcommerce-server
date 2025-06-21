using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.Entities;

public class ProductVariant : Entity
{
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; }
        public Guid? ColorId { get; private set; }
        public Guid? SizeId { get; private set; }
        public int StockQuantity { get; private set; }
        public Money AdditionalPrice { get; private set; }
        public string? ImageUrl { get; private set; } // ADICIONADO
        public bool IsActive { get; private set; }

        private ProductVariant() { }

        internal static ProductVariant NewVariant(
            Guid productId, string sku, int stockQuantity, Money additionalPrice,
            Guid? colorId, Guid? sizeId, string? imageUrl, bool isActive = true) // ADICIONADO
        {
            var variant = new ProductVariant
            {
                ProductId = productId, Sku = sku, StockQuantity = stockQuantity,
                AdditionalPrice = additionalPrice, ColorId = colorId,
                SizeId = sizeId, ImageUrl = imageUrl, IsActive = isActive // ADICIONADO
            };
            DomainException.ThrowWhen(string.IsNullOrWhiteSpace(sku), "SKU da variante é obrigatório.");
            DomainException.ThrowWhen(stockQuantity < 0, "Estoque da variante não pode ser negativo.");
            return variant;
        }

        public static ProductVariant With(
            Guid id, Guid productId, string sku, Guid? colorId, Guid? sizeId,
            int stockQuantity, Money additionalPrice, string? imageUrl, bool isActive) // ADICIONADO
        {
             return new ProductVariant {
                Id = id, ProductId = productId, Sku = sku, ColorId = colorId,
                SizeId = sizeId, StockQuantity = stockQuantity,
                AdditionalPrice = additionalPrice, ImageUrl = imageUrl, IsActive = isActive // ADICIONADO
            };
        }

        public void DecreaseStock(int quantityToDecrease)
        {
            DomainException.ThrowWhen(quantityToDecrease <= 0, "Quantidade a ser abatida deve ser positiva.");
            DomainException.ThrowWhen(StockQuantity < quantityToDecrease, $"Estoque insuficiente para o SKU {Sku}.");
            StockQuantity -= quantityToDecrease;
        }

        public void UpdateStock(int newQuantity)
        {
            DomainException.ThrowWhen(newQuantity < 0, "Estoque não pode ser negativo.");
            StockQuantity = newQuantity;
        }

        public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
    }