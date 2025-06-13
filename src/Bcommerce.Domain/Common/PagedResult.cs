namespace Bcommerce.Domain.Common;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);