using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;


public record AddItemToCartInput(Guid ProductVariantId, int Quantity);
