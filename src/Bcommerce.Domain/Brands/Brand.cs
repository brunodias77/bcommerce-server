using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bcommerce.Domain.Brands
{
    public class Brand : AggregateRoot
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Brand() { }

        public static Brand NewBrand(string name, string slug, string? description, string? logoUrl)
        {
            var brand = new Brand
            {
                Name = name,
                Slug = slug,
                Description = description,
                LogoUrl = logoUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Supondo que o Id é Guid e gerado pela classe base AggregateRoot
            brand.Validate(Notification.Create());
            return brand;
        }

        // Método para "hidratar" a entidade a partir do banco de dados
        public static Brand With(
            Guid id,
            string name,
            string slug,
            string? description,
            string? logoUrl,
            bool isActive,
            DateTime createdAt,
            DateTime updatedAt)
        {
            var brand = new Brand
            {
                Name = name,
                Slug = slug,
                Description = description,
                LogoUrl = logoUrl,
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
            brand.Id = id;
            return brand;
        }

            public void Update(string name, string slug, string? description, string? logoUrl, bool isActive)
            {
            Name = name;
            Slug = slug;
            Description = description;
            LogoUrl = logoUrl;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;

            Validate(Notification.Create());
        }

        public override void Validate(IValidationHandler handler)
        {
            new BrandValidator(this, handler).Validate();
        }
    }
}
