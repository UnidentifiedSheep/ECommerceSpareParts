using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;

namespace Main.Entities.Purchase;

public class PurchaseContent : Entity<PurchaseContent, int>
{
    [Validate]
    public int Id { get; private set; }
    public Guid PurchaseId { get; private set; }
    public int ProductId { get; private set; }
    public int? StorageContentId { get; private set; }
    public int Count { get; private set; }
    public decimal Price { get; private set; }
    public decimal TotalSum { get; private set; }
    public string? Comment { get; private set; }
    public Product.Product Product { get; private set; } = null!;
    public PurchaseContentLogistic? PurchaseContentLogistic { get; private set; }
    
    private PurchaseContent() {}

    private PurchaseContent(int productId, int count, decimal price, int? storageContentId)
    {
        ProductId = productId;
        StorageContentId = storageContentId;
        SetCount(count);
        SetPrice(price);
    }

    public static PurchaseContent Create(
        int productId,
        int count,
        decimal price,
        int? storageContentId,
        string? comment = null)
    {
        var item = new PurchaseContent(productId, count, price, storageContentId);
        item.SetComment(comment);
        return item;
    }

    public void SetCount(int count)
    {
        Count = count
            .AgainstLessOrEqual(
                min: 0, 
                exceptionFactory: () => new InvalidOperationException("Count must be greater than zero."));
        CalculateTotalSum();
    }

    public void SetPrice(decimal price)
    {
        Price = price
            .AgainstTooManyDecimalPlaces(
                maxDecimals: 2, 
                exceptionFactory: () => new InvalidOperationException("Price must have maximum 2 decimal places."))
            .AgainstLessOrEqual(
                min: 0,
                exceptionFactory: () => new InvalidOperationException("Price must be greater than zero."));
        CalculateTotalSum();
    }


    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()?
            .AgainstTooLong(
                max: 256,
                errorKey: "purchase.content.comment.too.long");
    }

    public void SetLogistic(decimal weightKg, decimal areaM3, decimal price)
    {
        if (PurchaseContentLogistic == null)
            PurchaseContentLogistic = PurchaseContentLogistic.Create(weightKg, areaM3, price);
        else
            PurchaseContentLogistic.Update(weightKg, areaM3, price);
    }
    
    private void CalculateTotalSum()
    {
        TotalSum = Price * Count;
    }
    
    public override int GetId() => Id;
}