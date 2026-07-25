using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Users;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.User;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseDtoProjectionProvider
    : IProjectionProvider<Purchase, PurchaseDto>
{
    public PurchaseDtoProjectionProvider(
        IProjectionProvider<PurchaseLogistic, PurchaseLogisticDto> logisticProjection,
        IProjectionProvider<Currency, CurrencyDto> currencyProjection,
        IProjectionProvider<User, UserDto> userProjection,
        IProjectionProvider<Organization, OrganizationDto> organizationProjection)
    {
        var logisticToDto = logisticProjection.Projection;
        var currencyToDto = currencyProjection.Projection;
        var userToDto = userProjection.Projection;
        var organizationToDto = organizationProjection.Projection;

        Projection = x => new PurchaseDto
        {
            Id = x.Id,
            Comment = x.Comment,
            Currency = currencyToDto.Invoke(x.Currency),
            Logistics = x.PurchaseLogistic == null
                ? null
                : logisticToDto.Invoke(x.PurchaseLogistic),
            PurchaseDatetime = x.PurchaseDatetime,
            Storage = x.Storage,
            Supplier = userToDto.Invoke(x.SupplierUser),
            SupplierOrganization = organizationToDto.Invoke(
                x.SupplierOrganization),
            TotalSum = x.Transaction.Amount,
            TransactionId = x.TransactionId
        };
    }

    public Expression<Func<Purchase, PurchaseDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseLogisticDtoProjectionProvider
    : IProjectionProvider<PurchaseLogistic, PurchaseLogisticDto>
{
    public PurchaseLogisticDtoProjectionProvider(
        IProjectionProvider<Currency, CurrencyDto> currencyProjection)
    {
        var currencyToDto = currencyProjection.Projection;

        Projection = x => new PurchaseLogisticDto
        {
            RouteId = x.RouteId,
            TransactionId = x.TransactionId,
            PricingModel = x.PricingModel,
            Currency = currencyToDto.Invoke(x.Currency),
            MinimumPrice = x.MinimumPrice,
            MinimumPriceApplied = x.MinimumPriceApplied,
            PriceKg = x.PriceKg,
            PricePerM3 = x.PricePerM3,
            PricePerOrder = x.PricePerOrder,
            RouteType = x.RouteType
        };
    }

    public Expression<Func<PurchaseLogistic, PurchaseLogisticDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseContentDtoProjectionProvider
    : IProjectionProvider<PurchaseContent, PurchaseContentDto>
{
    public PurchaseContentDtoProjectionProvider(
        IProjectionProvider<Product, ProductDto> productProjection,
        IProjectionProvider<PurchaseContentLogistic, PurchaseContentLogisticDto>
            contentLogisticProjection)
    {
        var productToDto = productProjection.Projection;
        var contentLogisticToDto = contentLogisticProjection.Projection;

        Projection = x => new PurchaseContentDto
        {
            Id = x.Id,
            Count = x.Count,
            Comment = x.Comment,
            Price = x.Price,
            TotalSum = x.TotalSum,
            Product = productToDto.Invoke(x.Product),
            ContentLogistics = x.PurchaseContentLogistic == null
                ? null
                : contentLogisticToDto.Invoke(x.PurchaseContentLogistic)
        };
    }

    public Expression<Func<PurchaseContent, PurchaseContentDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseContentLogisticDtoProjectionProvider
    : IProjectionProvider<PurchaseContentLogistic, PurchaseContentLogisticDto>
{
    public Expression<Func<PurchaseContentLogistic, PurchaseContentLogisticDto>> Projection { get; } =
        x => new PurchaseContentLogisticDto
        {
            WeightKg = x.WeightKg,
            AreaM3 = x.AreaM3,
            Price = x.Price
        };
}
