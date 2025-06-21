using Bcommerce.Domain.Customers.Clients.Enums;

namespace Bcommerce.Infrastructure.Data.Mappers;

public static class EnumMappers
{
    public static string ToDbString(this ClientStatus status) => status switch
    {
        ClientStatus.Active => "ativo",
        ClientStatus.Inactive => "inativo",
        ClientStatus.Banned => "banido",
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Status de cliente desconhecido: {status}")
    };
}