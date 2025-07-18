using Core.Extensions;
using Core.StaticFunctions;
using Mapster;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Dtos.Amw.Balances;
using MonoliteUnicorn.Dtos.Amw.Markups;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Dtos.Amw.Users;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Dtos.Member.Vehicles;
using MonoliteUnicorn.Dtos.Producers;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.RedisFunctions.Models;

using AmwArticleFullDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleFullDto = MonoliteUnicorn.Dtos.Member.Articles.ArticleFullDto;
using AmwArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = MonoliteUnicorn.Dtos.Anonymous.Articles.ArticleDto;

using AmwPurchaseDto = MonoliteUnicorn.Dtos.Amw.Purchase.PurchaseDto;
using MemberPurchaseDto = MonoliteUnicorn.Dtos.Member.Purchase.PurchaseDto;

namespace MonoliteUnicorn.Configs;

public static class MapsterConfig
{
    public static void Configure()
    {
        //Articles
            //ALL
        TypeAdapterConfig<Article, AmwArticleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProducerName, src => src.Producer.Name)
            .Map(dest => dest.Title, src => src.ArticleName)
            .Map(dest => dest.ArticleNumber, src => src.ArticleNumber)
            .Map(dest => dest.CurrentStock, src => src.TotalCount)
            .Map(dest => dest.Indicator, src => src.Indicator);
        
        TypeAdapterConfig<Article, AnonymousArticleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProducerName, src => src.Producer.Name)
            .Map(dest => dest.Title, src => src.ArticleName)
            .Map(dest => dest.ArticleNumber, src => src.ArticleNumber)
            .Map(dest => dest.CurrentStock, src => src.TotalCount);
        
            //AMW
        TypeAdapterConfig<NewArticleDto, Article>.NewConfig()
            .Map(d => d.ArticleName, s => s.Name.Trim())
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim())
            .Map(d => d.ArticleNumber, s => s.ArticleNumber.Trim())
            .Map(d => d.NormalizedArticleNumber, s => s.ArticleNumber.ToNormalizedArticleNumber())
            .Map(d => d.IsOe, s => s.IsOe)
            .Map(d => d.IsValid, s => true)
            .Map(d => d.PackingUnit, s => s.PackingUnit)
            .Map(d => d.Indicator, s => string.IsNullOrWhiteSpace(s.Indicator) ? null : s.Indicator.Trim())
            .Map(d => d.CategoryId, s => s.CategoryId);
        TypeAdapterConfig<Article, AmwArticleFullDto>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.ArticleNumber)
            .Map(d => d.Title, s => s.ArticleName)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.Producer.Name)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.ArticleImages.Select(x => x.Path))
            .Map(d => d.CurrentStock, s => s.TotalCount)
            .Map(d => d.IndicatorColor, s => s.Indicator);

        TypeAdapterConfig<Article, Versioned<AmwArticleFullDto>>.NewConfig()
            .ConstructUsing(src => new Versioned<AmwArticleFullDto>(src.Adapt<AmwArticleFullDto>()));
        
        TypeAdapterConfig<AmwArticleFullDto, MemberArticleFullDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.ArticleNumber)
            .Map(d => d.Title, s => s.Title)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.ProducerName)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.Images)
            .Map(d => d.CurrentStock, s => s.CurrentStock);
        
        TypeAdapterConfig<PatchArticleDto, Article>.NewConfig()
            .IgnorePatchIfNotSet()
            .IgnoreIf((src, dest) => !src.ArticleNumber.IsSet, dest => dest.NormalizedArticleNumber)
            .Map(d => d.ArticleName, s => s.ArticleName.Value)
            .Map(d => d.ProducerId, s => s.ProducerId.Value)
            .Map(d => d.Description, s => s.Description.Value)
            .Map(d => d.ArticleNumber, s => s.ArticleNumber.Value)
            .Map(d => d.NormalizedArticleNumber, s => s.ArticleNumber.Value!.ToNormalizedArticleNumber())
            .Map(d => d.IsOe, s => s.IsOe.Value)
            .Map(d => d.PackingUnit, s => s.PackingUnit.Value)
            .Map(d => d.Indicator, s => s.Indicator.Value)
            .Map(d => d.CategoryId, s => s.CategoryId.Value);
        
            //MEMBER
        TypeAdapterConfig<Article, MemberArticleFullDto>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.ArticleNumber)
            .Map(d => d.Title, s => s.ArticleName)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.Producer.Name)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.ArticleImages.Select(x => x.Path))
            .Map(d => d.CurrentStock, s => s.TotalCount);
        
        //Producers
        TypeAdapterConfig<Producer, ProducerDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
        TypeAdapterConfig<AmwNewProducerDto, Producer>.NewConfig()
            .Map(d => d.Name, s => s.ProducerName.Trim())
            .Map(d => d.IsOe, s => s.IsOe)
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim());
        TypeAdapterConfig<ProducersOtherName, ProducerOtherNameDto>.NewConfig()
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.OtherName, s => s.ProducerOtherName)
            .Map(d => d.WhereUsed, s => s.WhereUsed);
        TypeAdapterConfig<PatchProducerDto, Producer>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.Name, s => s.Name.Value == null ? null : s.Name.Value.Trim())
            .Map(d => d.Description, s => s.Description.Value == null ? null : s.Description.Value.Trim())
            .Map(d => d.IsOe, s => s.IsOe.Value);
            
        //Purchases
        TypeAdapterConfig<Purchase, AmwPurchaseDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SupplierId, src => src.SupplierId)
            .Map(dest => dest.TotalSum, src => src.Transaction.TransactionSum)
            .Map(dest => dest.TransactionId, src => src.TransactionId)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime)
            .Map(dest => dest.Comment, src => src.Comment);

        TypeAdapterConfig<EditPurchaseDto, PurchaseContent>
            .NewConfig()
            .IgnoreIf((src, dest) => src.Id == null, dest => dest.Id)
            .Map(d => d.Id, src => src.Id)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.TotalSum, s => s.Price * s.Count);
        
        //Users
        TypeAdapterConfig<AmwNewUserDto, AspNetUser>.NewConfig()
            .Ignore(x => x.Roles)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname)
            .Map(d => d.Email, s => s.Email)
            .Map(d => d.NormalizedEmail, s => (s.Email ?? "").ToUpperInvariant())
            .Map(d => d.UserName, s => s.UserName)
            .Map(d => d.NormalizedUserName, s => s.UserName.ToUpperInvariant())
            .Map(d => d.PhoneNumber, s => s.PhoneNumber)
            .Map(d => d.Id, s => Guid.NewGuid().ToString());
            //Vehicles
        TypeAdapterConfig<VehicleDto, UserVehicle>.NewConfig()
            .Map(d => d.Vin, s => s.Vin)
            .Map(d => d.PlateNumber, s => s.PlateNumber.Trim())
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.EngineCode, s => s.EngineCode)
            .Map(d => d.Manufacture, s => s.Manufacture.Trim())
            .Map(d => d.Model, s => s.Model.Trim())
            .Map(d => d.ProductionYear, s => s.ProductionYear)
            .Map(d => d.Modification, s => s.Modification);
            
            //Purchase
        TypeAdapterConfig<Sale, MemberPurchaseDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TotalSum, src => src.Transaction.TransactionSum)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.PurchaseDatetime, src => src.SaleDatetime);
        
        //Balances

        TypeAdapterConfig<Transaction, TransactionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SenderId, s => s.SenderId)
            .Map(dest => dest.ReceiverId, s => s.ReceiverId)
            .Map(dest => dest.Amount, s => s.TransactionSum)
            .Map(dest => dest.Status, s => s.Status)
            .Map(dest => dest.TransactionDate, dest => dest.TransactionDatetime)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId);

        TypeAdapterConfig<Transaction, TransactionVersion>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.TransactionId, src => src.Id)
            .Map(dest => dest.SenderId, s => s.SenderId)
            .Map(dest => dest.ReceiverId, s => s.ReceiverId)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId)
            .Map(dest => dest.Status, s => s.Status)
            .Map(dest => dest.TransactionDatetime, dest => DateTime.SpecifyKind(dest.TransactionDatetime, DateTimeKind.Unspecified))
            .Map(dest => dest.TransactionSum, dest => dest.TransactionSum);
        
        //STORAGES

        TypeAdapterConfig<StorageContent, StorageMovement>.NewConfig()
            .Ignore(x => x.WhoMovedNavigation)
            .Ignore(x => x.StorageNameNavigation)
            .Ignore(x => x.Article)
            .Ignore(x => x.Currency)
            .Ignore(x => x.Id)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.Price, s => s.BuyPrice)
            .Map(dest => dest.Count, s => s.Count)
            .Map(dest => dest.StorageName, s => s.StorageName)
            .Map(dest => dest.ArticleId, s => s.ArticleId);

        TypeAdapterConfig<StorageContent, StorageContent>.NewConfig()
            .Ignore(x => x.Article)
            .Ignore(x => x.Currency)
            .Ignore(x => x.StorageNameNavigation)
            .Ignore(x => x.SaleContentDetails)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.StorageName, s => s.StorageName)
            .Map(dest => dest.ArticleId, s => s.ArticleId)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId)
            .Map(dest => dest.Count, s => s.Count)
            .Map(dest => dest.BuyPrice, s => s.BuyPrice)
            .Map(dest => dest.BuyPriceInUsd, s => s.BuyPriceInUsd)
            .Map(dest => dest.CreatedDatetime, s => s.CreatedDatetime)
            .Map(dest => dest.PurchaseDatetime, s => s.PurchaseDatetime);
        
        TypeAdapterConfig<PatchStorageDto, Storage>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.Location, s => string.IsNullOrWhiteSpace(s.Location.Value) ? null : s.Location.Value.Trim())
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description.Value) ? null : s.Description.Value.Trim());

        TypeAdapterConfig<StorageContent, StorageContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.StorageName, src => src.StorageName)
            .Map(dest => dest.ConcurrencyCode, src => 
                    ConcurrencyStatic.GetConcurrencyCode(src.Id, src.ArticleId, src.BuyPrice, src.CurrencyId, src.StorageName, 
                        src.BuyPriceInUsd, src.Count, src.PurchaseDatetime));

        TypeAdapterConfig<PatchStorageContentDto, StorageContent>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(dest => dest.StorageName, src => src.StorageName.Value)
            .Map(dest => dest.BuyPrice, src => Math.Round(src.BuyPrice.Value, 2))
            .Map(dest => dest.CurrencyId, src => src.CurrencyId.Value)
            .Map(dest => dest.Count, src => src.Count.Value)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime.Value);
        //SALES
        TypeAdapterConfig<Sale, SaleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.BuyerId, src => src.BuyerId)
            .Map(dest => dest.TotalSum, src => src.Transaction.TransactionSum)
            .Map(dest => dest.TransactionId, src => src.TransactionId)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.SaleDatetime, src => src.SaleDatetime)
            .Map(dest => dest.Comment, src => src.Comment);


        TypeAdapterConfig<NewSaleContentDto, SaleContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.PriceWithDiscount)
            .Map(dest => dest.TotalSum, src => src.PriceWithDiscount * src.Count)
            .Map(dest => dest.Discount, src => (src.Price - src.PriceWithDiscount) / src.Price * 100);
        
        TypeAdapterConfig<EditSaleContentDto, SaleContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.PriceWithDiscount)
            .Map(dest => dest.TotalSum, src => src.PriceWithDiscount * src.Count)
            .Map(dest => dest.Discount, src => (src.Price - src.PriceWithDiscount) / src.Price * 100);

        
        //Остальные поля прописываются мануально
        TypeAdapterConfig<StorageContent, SaleContentDetail>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.StorageContentId, src => src.Id)
            .Map(dest => dest.Storage, src => src.StorageName)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime);

        //Остальные поля мануально прописываются BuyPriceInUsd, ArticleId, Status, Id
        TypeAdapterConfig<SaleContentDetail, StorageContent>.NewConfig()
            .Map(dest => dest.StorageName, src => src.Storage)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime)
            .Map(dest => dest.Count, src => src.Count);

        TypeAdapterConfig<SaleContentDetail, SaleContentDetail>.NewConfig()
            .Ignore(x => x.SaleContent)
            .Ignore(x => x.StorageNavigation);
        
        //User Search History

        TypeAdapterConfig<SearchLogModel, UserSearchHistory>.NewConfig()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Query, src => src.Query.ToString(Newtonsoft.Json.Formatting.None))
            .Map(dest => dest.SearchDateTime, src => src.SearchDateTime)
            .Map(dest => dest.SearchPlace, src => src.SearchPlace);

        //Article Reservation 
        TypeAdapterConfig<EditArticleReservationDto, StorageContentReservation>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment == null ? null : src.Comment.Trim())
            .Map(dest => dest.InitialCount, src => src.InitialCount)
            .Map(dest => dest.CurrentCount, src => src.CurrentCount)
            .Map(dest => dest.GivenCurrencyId, src => src.GivenCurrencyId)
            .Map(dest => dest.GivenPrice, src => src.GivenPrice);
        
        TypeAdapterConfig<NewArticleReservationDto, StorageContentReservation>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.GivenCurrencyId, src => src.GivenCurrencyId)
            .Map(dest => dest.GivenPrice, src => src.GivenPrice)
            .Map(dest => dest.CurrentCount, src => src.CurrentCount)
            .Map(dest => dest.InitialCount, src => src.InitialCount)
            .Map(dest => dest.UserId, src => src.UserId);

        //Markup

        TypeAdapterConfig<MarkupGroup, MarkupGroupDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.IsAutoGenerated, src => src.IsAutoGenerated)
            .Map(dest => dest.CurrencySign, src => src.Currency.CurrencySign);

        TypeAdapterConfig<MarkupRangeDto, MarkupRange>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Markup, src => src.Markup)
            .Map(dest => dest.RangeEnd, src => src.RangeEnd)
            .Map(dest => dest.RangeStart, src => src.RangeStart);

        TypeAdapterConfig<NewMarkupRangeDto, MarkupRange>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Markup, src => src.Markup)
            .Map(dest => dest.RangeEnd, src => src.RangeEnd)
            .Map(dest => dest.RangeStart, src => src.RangeStart);
    }
}