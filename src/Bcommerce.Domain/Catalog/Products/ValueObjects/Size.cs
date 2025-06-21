using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.ValueObjects;

public class Size : AggregateRoot
{
    public string Name { get; private set; }
    public string? SizeCode { get; private set; }
    public int SortOrder { get; private set; } // ADICIONADO
    public bool IsActive { get; private set; }

    private Size() {}

    public static Size NewSize(string name, string? sizeCode, int sortOrder)
    {
        return new Size { Name = name, SizeCode = sizeCode, SortOrder = sortOrder, IsActive = true };
    }
    
    // MÉTODO 'With' ADICIONADO PARA HIDRATAÇÃO
    public static Size With(Guid id, string name, string? sizeCode, int sortOrder, bool isActive)
    {
        var size = new Size
        {
            Id = id,
            Name = name,
            SizeCode = sizeCode,
            SortOrder = sortOrder,
            IsActive = isActive
        };
        return size;
    }

    public override void Validate(IValidationHandler handler) {}
}