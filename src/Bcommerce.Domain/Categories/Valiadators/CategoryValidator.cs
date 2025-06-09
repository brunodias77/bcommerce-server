using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Categories.Valiadators;

public class CategoryValidator : Validator
{
    private readonly Category _category;

    public CategoryValidator(Category category, IValidationHandler handler) : base(handler)
    {
        _category = category;
    }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_category.Name) || _category.Name.Length > 100)
        {
            ValidationHandler.Append(new Error("O nome da categoria é obrigatório e deve ter no máximo 100 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(_category.Slug) || _category.Slug.Length > 150)
        {
            ValidationHandler.Append(new Error("O slug da categoria é obrigatório e deve ter no máximo 150 caracteres."));
        }
    }
}