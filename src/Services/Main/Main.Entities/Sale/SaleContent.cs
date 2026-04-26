using Domain;
using Domain.Extensions;

namespace Main.Entities.Sale;

public class SaleContent : Entity<SaleContent, int>
{
    public int Id { get; private set; }

    public Guid SaleId { get; private set; }

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal Price { get; private set; }

    public decimal TotalSum { get; private set; }

    public string? Comment { get; private set; }

    public decimal Discount { get; private set; }

    public Product.Product Product { get; private set; } = null!;

    private readonly List<SaleContentDetail> _details = [];
    public IReadOnlyList<SaleContentDetail> Details => _details;
    
    private SaleContent() {}

    private SaleContent(
        int productId, 
        decimal priceWithOutDiscount, 
        decimal priceWithDiscount, 
        int count,
        IEnumerable<SaleContentDetail> details)
    {
        ProductId = productId;
        SetCount(count);
        SetPriceAndDetails(priceWithOutDiscount, priceWithDiscount, details);
    }

    public static SaleContent Create(
        int productId,
        decimal priceWithOutDiscount,
        decimal priceWithDiscount,
        int count,
        IEnumerable<SaleContentDetail> details)
    {
        return new SaleContent(productId, priceWithOutDiscount, priceWithDiscount, count, details);
    }

    public void SetCount(int count)
    {
        Count = count.AgainstLessOrEqual(0, "sale.content.count.min");
    }

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
        TotalSum = Price * Count;
        ClearAndSetDetails(details);
    }

    private void ClearAndSetDetails(IEnumerable<SaleContentDetail> details)
    {
        var list = details.ToList();
        int detailsCount = 0;
        HashSet<string> seenStorages = [];
        
        foreach (var detail in list)
        {
            seenStorages.Add(detail.Storage);
            detailsCount += detail.Count;
        }

        if (seenStorages.Count != 1)
            throw new InvalidOperationException("Sale content details must have only one storage");
        
        if (detailsCount != Count)
            throw new InvalidOperationException("Total details count is not equal to sale conent count");
        
        _details.Clear();
        _details.AddRange(list);
    }

    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()?
            .AgainstTooLong(256, "sale.content.comment.max");
    }

    public override int GetId() => Id;
}