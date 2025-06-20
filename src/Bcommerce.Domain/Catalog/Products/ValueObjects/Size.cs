using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.ValueObjects;

public class Size : AggregateRoot
{
    public string Name { get; private set; }
    public string? SizeCode { get; private set; }
    public bool IsActive { get; private set; }

    private Size() {}

    public static Size NewSize(string name, string? sizeCode)
    {
        return new Size { Name = name, SizeCode = sizeCode, IsActive = true };
    }

    public override void Validate(IValidationHandler handler) {}
}