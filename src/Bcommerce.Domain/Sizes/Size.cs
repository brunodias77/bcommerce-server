using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Sizes;

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

    public override void Validate(IValidationHandler handler) { /* ... new SizeValidator(this, handler) ... */ }
}