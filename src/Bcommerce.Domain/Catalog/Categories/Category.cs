using Bcommerce.Domain.Catalog.Categories.Events;
using Bcommerce.Domain.Catalog.Categories.Validators;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Categories;

public class Category : AggregateRoot
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() : base() { }

    public static Category NewCategory(
        string name, 
        string? description, 
        Guid? parentCategoryId, 
        IValidationHandler validationHandler)
    {
        var category = new Category
        {
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            Slug = GenerateSlug(name),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        category.Validate(validationHandler);

        if (!validationHandler.HasError())
        {
            category.RaiseEvent(new CategoryCreatedEvent(category.Id, category.Name));
        }
        
        return category;
    }

    public static Category With(
        Guid id,
        string name,
        string slug,
        string? description,
        bool isActive,
        Guid? parentCategoryId,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var category = new Category
        {
            Name = name,
            Slug = slug,
            Description = description,
            IsActive = isActive,
            ParentCategoryId = parentCategoryId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        category.Id = id;
        return category;
    }

    public override void Validate(IValidationHandler handler)
    {
        new CategoryValidator(this, handler).Validate();
    }
    
    public void Update(string name, string? description, IValidationHandler validationHandler)
    {
        Name = name;
        Description = description;
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;
        
        this.Validate(validationHandler);

        if (!validationHandler.HasError())
        {
            RaiseEvent(new CategoryUpdatedEvent(this.Id));
        }
    }
    
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new CategoryDeactivatedEvent(this.Id));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new CategoryActivatedEvent(this.Id));
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.ToLowerInvariant().Replace(" ", "-");
    }
}