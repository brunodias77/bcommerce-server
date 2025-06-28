using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;

public record CartOutput(Guid CartId, Guid ClientId, decimal TotalCartPrice, IReadOnlyCollection<CartItemOutput> Items);
