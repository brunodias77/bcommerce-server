using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Catalog.Products.Validators;

public class ProductValidator : Validator
{
    private readonly Product _product;
    public ProductValidator(Product product, IValidationHandler handler) : base(handler)
    {
        _product = product;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_product.Name))
        {
            ValidationHandler.Append(new Error("'Name' do produto é obrigatório."));
        }
        if (string.IsNullOrWhiteSpace(_product.BaseSku))
        {
            ValidationHandler.Append(new Error("'BaseSku' do produto é obrigatório."));
        }
        if (_product.BasePrice is null || _product.BasePrice.Amount <= 0)
        {
            ValidationHandler.Append(new Error("Produto deve ter um preço base positivo."));
        }
    }
}