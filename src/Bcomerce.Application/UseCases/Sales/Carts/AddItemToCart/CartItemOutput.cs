using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;

public record CartItemOutput(Guid CartItemId, Guid ProductVariantId, string ItemName, int Quantity, decimal UnitPrice, decimal TotalPrice);
