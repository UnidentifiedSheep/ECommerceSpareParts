using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.Storage;

namespace Main.Entities.Purchase;

public class PurchaseContent : Entity<PurchaseContent, int>, ILinqEntity<PurchaseContent, int>
{
    private PurchaseContent() { }

    private PurchaseContent(
        int productId,
        int count,
        decimal price,
        int storageContentId)
    {
        ProductId = productId;
        StorageContentId = storageContentId;
        SetCount(count);
        SetPrice(price);
    }

    [Validate]
    public int Id { get; private set; }

    public Guid PurchaseId { get; private set; }
    public int ProductId { get; private set; }
    public int StorageContentId { get; private set; }
    public int Count { get; private set; }
    public decimal Price { get; private set; }
    public decimal TotalSum { get; private set; }
    public string? Comment { get; private set; }
    public Product.Product Product { get; private set; } = null!;
    public PurchaseContentLogistic? PurchaseContentLogistic { get; private set; }
    public StorageContent StorageContent { get; private set; } = null!;

    public static Expression<Func<PurchaseContent, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<PurchaseContent, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static PurchaseContent Create(
        int productId,
        int count,
        decimal price,
        int storageContentId,
        string? comment = null)
    {
        var item = new PurchaseContent(
            productId,
            count,
            price,
            storageContentId);
        item.SetComment(comment);
        return item;
    }

    public void SetCount(int count)
    {
        Count = count
            .EnsureGreaterThan(
                0,
                () => new InvalidOperationException("Count must be greater than zero."));
        CalculateTotalSum();
    }

    public void SetPrice(decimal price)
    {
        Price = price
            .EnsureMaxDecimalPlaces(
                2,
                () => new InvalidOperationException("Price must have maximum 2 decimal places."))
            .EnsureGreaterThan(
                0,
                () => new InvalidOperationException("Price must be greater than zero."));
        CalculateTotalSum();
    }

    public void SetPurchaseId(Guid purchaseId) { PurchaseId = purchaseId; }

    public void SetStorageContentId(int storageContentId) { StorageContentId = storageContentId; }

    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()
            ?
            .EnsureMaxLength(
                256,
                "purchase.content.comment.too.long");
    }

    public void SetLogistic(
        decimal weightKg,
        decimal areaM3,
        decimal price)
    {
        if (PurchaseContentLogistic == null)
            PurchaseContentLogistic = PurchaseContentLogistic.Create(
                weightKg,
                areaM3,
                price);
        else
            PurchaseContentLogistic.Update(
                weightKg,
                areaM3,
                price);
    }

    public PurchaseContentLogistic? ClearLogistic()
    {
        var logistic = PurchaseContentLogistic;
        PurchaseContentLogistic = null;
        return logistic;
    }

    private void CalculateTotalSum() { TotalSum = Price * Count; }

    public override int GetId() { return Id; }
}