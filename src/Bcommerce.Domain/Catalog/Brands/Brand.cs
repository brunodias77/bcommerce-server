using Bcommerce.Domain.Catalog.Brands.Events;
using Bcommerce.Domain.Catalog.Brands.Validators;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Brands;

public class Brand : AggregateRoot
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private Brand() : base() { }
    
    public static Brand NewBrand(
        string name,
        string? description,
        string? logoUrl,
        IValidationHandler validationHandler)
    {
        var brand = new Brand
        {
            Name = name,
            Description = description,
            LogoUrl = logoUrl,
            Slug = GenerateSlug(name),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        brand.Validate(validationHandler);

        if (!validationHandler.HasError())
        {
            brand.RaiseEvent(new BrandCreatedEvent(brand.Id, brand.Name));
        }
        
        return brand;
    }
    
    public static Brand With(
        Guid id, string name, string slug, string? description, 
        string? logoUrl, bool isActive, DateTime createdAt, DateTime updatedAt)
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


    
    public override void Validate(IValidationHandler handler)
    {
        new BrandValidator(this, handler).Validate();
    }
    
    public void Update(
        string name, 
        string? description, 
        string? logoUrl, 
        IValidationHandler validationHandler)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;

        this.Validate(validationHandler);
        if (!validationHandler.HasError())
        {
            RaiseEvent(new BrandUpdatedEvent(this.Id));
        }
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.ToLowerInvariant().Replace(" ", "-");
    }
}