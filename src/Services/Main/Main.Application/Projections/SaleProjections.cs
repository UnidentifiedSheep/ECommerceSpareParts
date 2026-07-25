using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Sale;
using Main.Application.Dtos.Users;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Entities.User;

namespace Main.Application.Projections;

[LifetimeAttribute(Lifetime.Singleton)]
public sealed class SaleDtoProjectionProvider
    : IProjectionProvider<Sale, SaleDto>
{
    public SaleDtoProjectionProvider(
        IProjectionProvider<User, UserDto> userProjection,
        IProjectionProvider<Organization, OrganizationDto> organizationProjection,
        IProjectionProvider<Currency, CurrencyDto> currencyProjection)
    {
        var userToDto = userProjection.Projection;
        var organizationToDto = organizationProjection.Projection;
        var currencyToDto = currencyProjection.Projection;

        Projection = x => new SaleDto
        {
            Id = x.Id,
            Buyer = userToDto.Invoke(x.User),
            Organization = organizationToDto.Invoke(x.Organization),
            Comment = x.Comment,
            Currency = currencyToDto.Invoke(x.Currency),
            SaleDatetime = x.SaleDatetime,
            Storage = x.StorageName,
            TotalSum = x.Transaction.Amount,
            TransactionId = x.TransactionId,
            RowVersion = x.RowVersion,
            State = x.State
        };
    }

    public Expression<Func<Sale, SaleDto>> Projection { get; }
}

[LifetimeAttribute(Lifetime.Singleton)]
public sealed class SaleContentDtoProjectionProvider
    : IProjectionProvider<SaleContent, SaleContentDto>
{
    public SaleContentDtoProjectionProvider(
        IProjectionProvider<Product, ProductDto> productProjection,
        IProjectionProvider<SaleContentDetail, SaleContentDetailDto> detailProjection)
    {
        var productToDto = productProjection.Projection;
        var detailToDto = detailProjection.Projection;

        Projection = x => new SaleContentDto
        {
            Id = x.Id,
            Count = x.Count,
            Price = x.Price,
            TotalSum = x.TotalSum,
            Discount = x.Discount,
            Comment = x.Comment,
            Product = productToDto.Invoke(x.Product),
            Details = x.Details.Select(z => detailToDto.Invoke(z)).ToList()
        };
    }

    public Expression<Func<SaleContent, SaleContentDto>> Projection { get; }
}

[LifetimeAttribute(Lifetime.Singleton)]
public sealed class SaleContentDetailDtoProjectionProvider
    : IProjectionProvider<SaleContentDetail, SaleContentDetailDto>
{
    public SaleContentDetailDtoProjectionProvider(
        IProjectionProvider<Currency, CurrencyDto> currencyProjection)
    {
        var currencyToDto = currencyProjection.Projection;

        Projection = x => new SaleContentDetailDto
        {
            Id = x.Id,
            BuyPrice = x.BuyPrice,
            Count = x.Count,
            Currency = currencyToDto.Invoke(x.Currency),
            PurchaseDatetime = x.PurchaseDatetime,
            SaleContentId = x.SaleContentId,
            StorageContentId = x.StorageContentId
        };
    }

    public Expression<Func<SaleContentDetail, SaleContentDetailDto>> Projection { get; }
}
