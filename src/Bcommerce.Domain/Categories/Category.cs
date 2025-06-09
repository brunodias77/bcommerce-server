using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Categories.Valiadators;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcommerce.Domain.Categories;
public class Category : AggregateRoot
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Construtor para o Dapper/EF Core
    private Category() {}

    public static Category NewCategory(string name, string slug, string? description, Guid? parentCategoryId)
    {
        var category = new Category
        {
            Name = name,
            Slug = slug,
            Description = description,
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        category.Validate(Notification.Create());
        return category;
    }
    
    public static Category With(
        Guid id,
        string name,
        string slug,
        string? description,
        Guid? parentCategoryId,
        bool isActive,
        int sortOrder,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new Category
        {
            Id = id,
            Name = name,
            Slug = slug,
            Description = description,
            ParentCategoryId = parentCategoryId,
            IsActive = isActive,
            SortOrder = sortOrder,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Update(string name, string slug, string? description, Guid? parentCategoryId, bool isActive, int sortOrder)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = isActive;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
        
        Validate(Notification.Create());
    }
    
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public override void Validate(IValidationHandler handler)
    {
        new CategoryValidator(this, handler).Validate();
    }
}