using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bcommerce.Domain.Brands.Validators
{
    class BrandValidator : Validator
    {
        private readonly Brand _brand;

        public BrandValidator(Brand brand, IValidationHandler handler) : base(handler)
        {
            _brand = brand;
        }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(_brand.Name) || _brand.Name.Length > 100)
            {
                ValidationHandler.Append(new Error("O nome da marca é obrigatório e deve ter no máximo 100 caracteres."));
            }

            if (string.IsNullOrWhiteSpace(_brand.Slug) || _brand.Slug.Length > 150)
            {
                ValidationHandler.Append(new Error("O slug da marca é obrigatório e deve ter no máximo 150 caracteres."));
            }
        }
    }
}
