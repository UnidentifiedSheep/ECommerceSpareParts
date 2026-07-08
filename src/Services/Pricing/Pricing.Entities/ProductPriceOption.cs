using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities;

public class ProductPriceOption : AuditableEntity<ProductPriceOption, Guid>, ILinqEntity<ProductPriceOption, Guid>
{
    public Guid Id { get; private set; }
    public Guid PriceOfferId { get; private set; }
    
    public int ProductId { get; private set; }
    public string StorageName { get; private set; } = string.Empty;
    public int Rank { get; private set; }
    
    public decimal PriceInBaseCurrency { get; private set; }
    public int BaseCurrencyId { get; private set; }
    public decimal Markup { get; private set; }
    public int AvailableQuantity { get; private set; }
    
    public PriceOptionType Type { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    
    public override Guid GetId() => Id;
    public static Expression<Func<ProductPriceOption, Guid>> GetKeySelector() => x => x.Id;
    public static Expression<Func<ProductPriceOption, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;
}