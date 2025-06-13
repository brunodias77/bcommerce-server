using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Colors;

public class Color : AggregateRoot
{
    public string Name { get; private set; }
    public string? HexCode { get; private set; }
    public bool IsActive { get; private set; }

    private Color() {}

    public static Color NewColor(string name, string? hexCode)
    {
        // Validação do HexCode pode ser adicionada no Validator
        return new Color { Name = name, HexCode = hexCode, IsActive = true };
    }

    public override void Validate(IValidationHandler handler) { /* ... new ColorValidator(this, handler) ... */ }
}