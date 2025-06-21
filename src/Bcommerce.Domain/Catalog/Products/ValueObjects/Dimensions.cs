using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;

namespace Bcommerce.Domain.Catalog.Products.ValueObjects;

public class Dimensions : ValueObject
{
    public decimal? WeightKg { get; }
    public int? HeightCm { get; }
    public int? WidthCm { get; }
    public int? DepthCm { get; }
    
    private Dimensions(decimal? weightKg, int? heightCm, int? widthCm, int? depthCm)
    {
        WeightKg = weightKg;
        HeightCm = heightCm;
        WidthCm = widthCm;
        DepthCm = depthCm;
    }
    
    public static Dimensions Create(decimal? weightKg, int? heightCm, int? widthCm, int? depthCm)
    {
        DomainException.ThrowWhen(weightKg.HasValue && weightKg <= 0, "Peso deve ser positivo.");
        DomainException.ThrowWhen(heightCm.HasValue && heightCm <= 0, "Altura deve ser positiva.");
        DomainException.ThrowWhen(widthCm.HasValue && widthCm <= 0, "Largura deve ser positiva.");
        DomainException.ThrowWhen(depthCm.HasValue && depthCm <= 0, "Profundidade deve ser positiva.");

        return new Dimensions(weightKg, heightCm, widthCm, depthCm);
    }
    public override bool Equals(ValueObject? other)
    {
        if (other is not Dimensions d) return false;
        return WeightKg == d.WeightKg && HeightCm == d.HeightCm && WidthCm == d.WidthCm && DepthCm == d.DepthCm;
    }

    protected override int GetCustomHashCode() => HashCode.Combine(WeightKg, HeightCm, WidthCm, DepthCm);

}