using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Products.Validators;

public class ProductValidator : Validator
{
    private readonly Product _product;

    public ProductValidator(Product product, IValidationHandler handler) : base(handler)
    {
        _product = product;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_product.Name) || _product.Name.Length > 150)
        {
            ValidationHandler.Append(new Error("O nome do produto é obrigatório e deve ter no máximo 150 caracteres."));
        }

        if (string.IsNullOrWhiteSpace(_product.Slug) || _product.Slug.Length > 200)
        {
            ValidationHandler.Append(new Error("O slug do produto é obrigatório e deve ter no máximo 200 caracteres."));
        }

        if (_product.BasePrice <= 0)
        {
            ValidationHandler.Append(new Error("O preço base do produto deve ser maior que zero."));
        }

        if (_product.CategoryId == Guid.Empty)
        {
            ValidationHandler.Append(new Error("A categoria do produto é obrigatória."));
        }
    }
}