using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.ValueObjects;

public class Color : AggregateRoot // Pode ser um Agregado se tiver um ciclo de vida e regras próprias
{
    public string Name { get; private set; }
    public string? HexCode { get; private set; }
    public bool IsActive { get; private set; }

    // Construtores e métodos de fábrica aqui
    private Color() {}
        
    public static Color NewColor(string name, string? hexCode)
    {
        return new Color { Name = name, HexCode = hexCode, IsActive = true };
    }

    public override void Validate(IValidationHandler handler) {}
}