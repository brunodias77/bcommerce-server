using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Categories.Validators;

public class CategoryValidator : Validator
{
    private readonly Category _category;

    public CategoryValidator(Category category, IValidationHandler handler) : base(handler)
    {
        _category = category;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_category.Name))
        {
            ValidationHandler.Append(new Error("'Name' não pode ser nulo ou vazio."));
        }

        if (_category.Name?.Length > 100)
        {
            ValidationHandler.Append(new Error("'Name' não pode ter mais de 100 caracteres."));
        }
    }
}