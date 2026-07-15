using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Purchase;

public class PurchaseContentLogistic : Entity<PurchaseContentLogistic, int>,
    ILinqEntity<PurchaseContentLogistic, int>
{
    private PurchaseContentLogistic() { }

    private PurchaseContentLogistic(
        decimal weightKg,
        decimal areaM3,
        decimal price)
    {
        SetWeightKg(weightKg);
        SetAreaM3(areaM3);
        SetPrice(price);
    }

    public int PurchaseContentId { get; private set; }
    public decimal WeightKg { get; private set; }
    public decimal AreaM3 { get; private set; }
    public decimal Price { get; private set; }

    public static Expression<Func<PurchaseContentLogistic, int>> GetKeySelector()
    {
        return x => x.PurchaseContentId;
    }

    public static Expression<Func<PurchaseContentLogistic, bool>> GetEqualityExpression(int key)
    {
        return x => x.PurchaseContentId == key;
    }

    internal static PurchaseContentLogistic Create(
        decimal weightKg,
        decimal areaM3,
        decimal price)
    {
        return new PurchaseContentLogistic(
            weightKg,
            areaM3,
            price);
    }

    internal void Update(
        decimal weightKg,
        decimal areaM3,
        decimal price)
    {
        SetWeightKg(weightKg);
        SetAreaM3(areaM3);
        SetPrice(price);
    }

    private void SetWeightKg(decimal weightKg)
    {
        WeightKg = weightKg.EnsureNonNegative(() =>
            throw new InvalidOperationException("Purchase content logistics weight must be positive"));
    }

    private void SetAreaM3(decimal areaM3)
    {
        AreaM3 = areaM3.EnsureNonNegative(() =>
            throw new InvalidOperationException("Purchase content logistics area m3 must be positive"));
    }

    private void SetPrice(decimal price)
    {
        Price = price.EnsureNonNegative(() =>
            throw new InvalidOperationException("Purchase content logistics price must be positive"));
    }

    public override int GetId() { return PurchaseContentId; }
}