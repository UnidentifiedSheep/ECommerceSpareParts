using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Sale;

public class SaleContent : Entity<SaleContent, int>, ILinqEntity<SaleContent, int>
{
    private readonly List<SaleContentDetail> _details = [];

    private SaleContent() { }

    private SaleContent(
        int productId,
        decimal priceWithOutDiscount,
        decimal priceWithDiscount,
        IEnumerable<SaleContentDetail> details)
    {
        ProductId = productId;
        SetPriceAndDetails(
            priceWithOutDiscount,
            priceWithDiscount,
            details);
    }

    public int Id { get; private set; }

    public Guid SaleId { get; private set; }

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal Price { get; private set; }

    public decimal TotalSum { get; private set; }

    public string? Comment { get; private set; }

    public decimal Discount { get; private set; }

    public Product.Product Product { get; private set; } = null!;
    public IReadOnlyList<SaleContentDetail> Details => _details;

    public static Expression<Func<SaleContent, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<SaleContent, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static SaleContent Create(
        int productId,
        decimal priceWithOutDiscount,
        decimal priceWithDiscount,
        IEnumerable<SaleContentDetail> details)
    {
        return new SaleContent(
            productId,
            priceWithOutDiscount,
            priceWithDiscount,
            details);
    }

    private void SetCount(int count) { Count = count.AgainstLessOrEqual(0, "sale.content.count.min"); }

    public void SetPriceAndDetails(
        decimal withOutDiscount,
        decimal withDiscount,
        IEnumerable<SaleContentDetail> details)
    {
        withOutDiscount
            .AgainstTooManyDecimalPlaces(2, "sale.content.price.precision")
            .AgainstLessOrEqual(0, "sale.content.price.min");

        Price = withDiscount
            .AgainstTooManyDecimalPlaces(2, "sale.content.price.with.discount.precision")
            .AgainstLessOrEqual(0, "sale.content.price.with.discount.min")
            .AgainstTooBig(withOutDiscount, "sale.content.price.with.discount.max");

        Discount = (withOutDiscount - withDiscount) / withOutDiscount;
        ClearAndSetDetails(details);
        TotalSum = Price * Count;
    }

    private void ClearAndSetDetails(IEnumerable<SaleContentDetail> details)
    {
        var list = details.ToList();
        var detailsCount = list.Sum(detail => detail.Count);

        SetCount(detailsCount);

        _details.Clear();
        _details.AddRange(list);
    }

    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()
            ?
            .AgainstTooLong(256, "sale.content.comment.max");
    }

    public override int GetId() { return Id; }
}