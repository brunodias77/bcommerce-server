using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Brands.Validators;

public class BrandValidator : Validator
{
    private readonly Brand _brand;
    
    public BrandValidator(Brand brand, IValidationHandler handler) : base(handler)
    {
        _brand = brand;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_brand.Name))
        {
            ValidationHandler.Append(new Error("'Name' da marca é obrigatório."));
        }
        if (_brand.Name?.Length > 100)
        {
            ValidationHandler.Append(new Error("'Name' da marca não pode ter mais de 100 caracteres."));
        }
        if (!string.IsNullOrEmpty(_brand.LogoUrl) && !Uri.TryCreate(_brand.LogoUrl, UriKind.Absolute, out _))
        {
            ValidationHandler.Append(new Error("'LogoUrl' deve ser uma URL válida."));
        }
    }
}