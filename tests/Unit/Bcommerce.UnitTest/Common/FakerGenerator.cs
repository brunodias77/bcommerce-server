using Bogus;

namespace Bcommerce.UnitTest.Common;

/// <summary>
/// Fornece uma instância estática e centralizada do Bogus.Faker para ser
/// reutilizada em todos os projetos de teste, garantindo consistência.
/// </summary>
public static class FakerGenerator
{
    public static Faker Faker { get; } = new("pt_BR");
}