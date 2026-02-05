using System.Text.Json;
using Core.Extensions;
using Core.Models;
using Core.StaticFunctions;
using Main.Abstractions.Dtos.Amw.ArticleCharacteristics;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Dtos.Amw.Balances;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.Markups;
using Main.Abstractions.Dtos.Amw.Permissions;
using Main.Abstractions.Dtos.Amw.Producers;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Anonymous.Articles;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Main.Abstractions.Dtos.ArticleSizes;
using Main.Abstractions.Dtos.ArticleWeight;
using Main.Abstractions.Dtos.Cart;
using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Member.Vehicles;
using Main.Abstractions.Dtos.Roles;
using Main.Abstractions.Dtos.Services.Articles;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Models.Logistics;
using Main.Abstractions.Models.Pricing;
using Main.Application.Extensions;
using Main.Application.Handlers.ArticlePairs.CreatePair;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Entities;
using Mapster;
using AmwArticleFullDto = Main.Abstractions.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleFullDto = Main.Abstractions.Dtos.Member.Articles.ArticleFullDto;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Abstractions.Dtos.Anonymous.Articles.ArticleDto;
using AmwPurchaseDto = Main.Abstractions.Dtos.Amw.Purchase.PurchaseDto;
using MemberPurchaseDto = Main.Abstractions.Dtos.Member.Purchase.PurchaseDto;

using CoreUser = Core.Models.User;
using User = Main.Entities.User;
using CoreUserInfo = Core.Models.UserInfo;
using Currency = Main.Entities.Currency;
using UserInfo = Main.Entities.UserInfo;

namespace Main.Application.Configs;

public static class MapsterConfig
{
    public static void Configure()
    {

        TypeAdapterConfig<User, CoreUser>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.UserName, s => s.UserName);

        TypeAdapterConfig<UserInfo, CoreUserInfo>.NewConfig()
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname);
        
        TypeAdapterConfig<PurchaseContent, PurchaseContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Article, src => src.Article)
            .Map(dest => dest.TotalSum, src => src.TotalSum)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Count, src => src.Count);

        TypeAdapterConfig<NewPurchaseContentDto, PurchaseContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.TotalSum, src => src.Price * src.Count)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(d => d.StorageContentId, s => s.StorageContentId);

        TypeAdapterConfig<NewPurchaseContentDto, NewStorageContentDto>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.BuyPrice, src => src.Price);

        TypeAdapterConfig<NewPurchaseContentDto, LogisticsItemDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Quantity, s => s.Count);

        TypeAdapterConfig<PurchaseContentLogisticDto, PurchaseContentLogistic>.NewConfig()
            .Map(d => d.AreaM3, s => s.AreaM3)
            .Map(d => d.PurchaseContentId, s => s.PurchaseContentId)
            .Map(d => d.WeightKg, s => s.WeightKg)
            .Map(d => d.Price, s => s.Price);
        
        TypeAdapterConfig<PurchaseContentLogistic, PurchaseContentLogisticDto>.NewConfig()
            .Map(d => d.AreaM3, s => s.AreaM3)
            .Map(d => d.PurchaseContentId, s => s.PurchaseContentId)
            .Map(d => d.WeightKg, s => s.WeightKg)
            .Map(d => d.Price, s => s.Price);


        TypeAdapterConfig<NewStorageContentDto, StorageContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.PurchaseDatetime,
                src => src.PurchaseDate);

        TypeAdapterConfig<CreatePairCommand, ArticlesPair>.NewConfig()
            .Map(dest => dest.ArticleLeft, src => src.LeftArticleId)
            .Map(dest => dest.ArticleRight, src => src.RightArticleId);
        TypeAdapterConfig<NewCharacteristicsDto, ArticleCharacteristic>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Name, src => src.Name == null ? null : src.Name.Trim())
            .Map(dest => dest.Value, src => src.Value.Trim());
        //Articles
        //ALL
        TypeAdapterConfig<Article, AmwArticleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProducerId, src => src.ProducerId)
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

        TypeAdapterConfig<ArticlesContent, ContentArticleDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Quantity, src => src.Quantity)
            .Map(d => d.Article, s => s.InsideArticle);

        //AMW
        TypeAdapterConfig<CreateArticleDto, Article>.NewConfig()
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

        TypeAdapterConfig<PatchCharacteristicsDto, ArticleCharacteristic>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.Name, s => s.Name.Value == null ? null : s.Name.Value.Trim())
            .Map(d => d.Value, s => s.Value.Value == null ? null : s.Value.Value.Trim());

        TypeAdapterConfig<CreateStorageCommand, Storage>.NewConfig()
            .Map(d => d.Name, s => s.Name.Trim())
            .Map(d => d.Description, s => s.Description == null ? null : s.Description.Trim())
            .Map(d => d.Location, s => s.Location == null ? null : s.Location.Trim())
            .Map(d => d.Type, s => s.Type.ToString());

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
            .Map(dest => dest.IsOe, src => src.IsOe)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
        TypeAdapterConfig<NewProducerDto, Producer>.NewConfig()
            .Map(d => d.Name, s => s.ProducerName.ToNormalized())
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
            .Map(dest => dest.Storage, s => s.Storage)
            .Map(dest => dest.Supplier, src => src.Supplier)
            .Map(dest => dest.TotalSum, src => src.Transaction.TransactionSum)
            .Map(dest => dest.TransactionId, src => src.TransactionId)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime)
            .Map(dest => dest.Comment, src => src.Comment);
        
        
        TypeAdapterConfig<StorageRoute, PurchaseLogistic>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.RouteId, s => s.Id)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.PriceKg, s => s.PriceKg)
            .Map(d => d.PricePerM3, s => s.PricePerM3)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder)
            .Map(d => d.RouteType, s => s.RouteType)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice)
            .Map(d => d.PricingModel, s => s.PricingModel);

        TypeAdapterConfig<EditPurchaseDto, PurchaseContent>
            .NewConfig()
            .IgnoreIf((src, dest) => src.Id == null, dest => dest.Id)
            .Map(d => d.Id, src => src.Id)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.TotalSum, s => s.Price * s.Count);
        
        TypeAdapterConfig<PurchaseLogistic, PurchaseLogisticDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.PurchaseId, s => s.PurchaseId)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.CurrencySign, s => s.Currency.CurrencySign)
            .Map(d => d.CurrencyName, s => s.Currency.Name)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice)
            .Map(d => d.MinimumPriceApplied, s => s.MinimumPriceApplied)
            .Map(d => d.PriceKg, s => s.PriceKg)
            .Map(d => d.PricePerM3, s => s.PricePerM3)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder)
            .Map(d => d.PricingModel, s => s.PricingModel)
            .Map(d => d.RouteId, s => s.RouteId)
            .Map(d => d.RouteType, s => s.RouteType)
            .Map(d => d.TransactionId, s => s.TransactionId);

        //Users
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(d => d.Name, s => s.UserInfo == null ? "UNKNOWN" : s.UserInfo.Name)
            .Map(d => d.Surname, s => s.UserInfo == null ? "UNKNOWN" : s.UserInfo.Surname)
            .Map(d => d.UserName, s => s.UserName)
            .Map(d => d.IsSupplier, s => s.UserInfo != null && s.UserInfo.IsSupplier)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Description, s => s.UserInfo == null ? null : s.UserInfo.Description);

        TypeAdapterConfig<UserInfoDto, UserInfo>.NewConfig()
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname)
            .Map(d => d.IsSupplier, s => s.IsSupplier)
            .Map(d => d.SearchColumn, s => $"{s.Name} {s.Surname} {s.Description}".ToNormalized());

        TypeAdapterConfig<UserInfo, UserInfoDto>.NewConfig()
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname)
            .Map(d => d.IsSupplier, s => s.IsSupplier);
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


        //Roles

        TypeAdapterConfig<Role, RoleDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsSystem, src => src.IsSystem);
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
            .Map(dest => dest.TransactionDatetime,
                dest => dest.TransactionDatetime)
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
            .Map(d => d.Description,
                s => string.IsNullOrWhiteSpace(s.Description.Value) ? null : s.Description.Value.Trim())
            .Map(d => d.Type, s => s.Type.Value);

        TypeAdapterConfig<StorageContent, StorageContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.StorageName, src => src.StorageName)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.ConcurrencyCode, src =>
                ConcurrencyStatic.GetConcurrencyCode(src.Id, src.ArticleId, src.BuyPrice, src.CurrencyId,
                    src.StorageName, src.BuyPriceInUsd, src.Count, src.PurchaseDatetime));

        TypeAdapterConfig<PatchStorageContentDto, StorageContent>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(dest => dest.BuyPrice, src => Math.Round(src.BuyPrice.Value, 2))
            .Map(dest => dest.CurrencyId, src => src.CurrencyId.Value)
            .Map(dest => dest.Count, src => src.Count.Value)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime.Value);
        //SALES
        TypeAdapterConfig<Sale, SaleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Buyer, src => src.Buyer)
            .Map(dest => dest.TotalSum, src => src.Transaction.TransactionSum)
            .Map(dest => dest.TransactionId, src => src.TransactionId)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.SaleDatetime, src => src.SaleDatetime)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Storage, src => src.MainStorageName);

        TypeAdapterConfig<SaleContent, SaleContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(d => d.Article, s => s.Article)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Discount, s => s.Discount)
            .Map(d => d.TotalSum, s => s.TotalSum);

        TypeAdapterConfig<Storage, StorageDto>.NewConfig()
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Location, s => s.Location)
            .Map(d => d.Type, s => s.Type);

        TypeAdapterConfig<NewSaleContentDto, SaleContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.PriceWithDiscount)
            .Map(dest => dest.TotalSum, src => src.PriceWithDiscount * src.Count)
            .Map(dest => dest.Discount, 
                src => Price.GetDiscountFromPrices(src.PriceWithDiscount, src.Price));

        TypeAdapterConfig<EditSaleContentDto, SaleContent>.NewConfig()
            .Map(dest => dest.ArticleId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.PriceWithDiscount)
            .Map(dest => dest.TotalSum, src => src.PriceWithDiscount * src.Count)
            .Map(dest => dest.Discount, 
                src => Price.GetDiscountFromPrices(src.PriceWithDiscount, src.Price));

        TypeAdapterConfig<Sale, Contracts.Models.Sale.Sale>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.BuyerId, s => s.BuyerId)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.CreatedUserId, s => s.CreatedUserId)
            .Map(d => d.CreationDatetime, s => s.CreationDatetime)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.MainStorageName, s => s.MainStorageName)
            .Map(d => d.SaleContents, s => s.SaleContents)
            .Map(d => d.SaleDatetime, s => s.SaleDatetime)
            .Map(d => d.TransactionId, s => s.TransactionId);

        TypeAdapterConfig<SaleContent, Contracts.Models.Sale.SaleContent>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.Details, s => s.SaleContentDetails)
            .Map(d => d.Discount, s => s.Discount)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.SaleId, s => s.SaleId)
            .Map(d => d.TotalSum, s => s.TotalSum);

        TypeAdapterConfig<SaleContentDetail, Contracts.Models.Sale.SaleContentDetail>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.BuyPrice, s => s.BuyPrice)
            .Map(d => d.PurchaseDatetime, s => s.PurchaseDatetime)
            .Map(d => d.SaleContentId, s => s.SaleContentId)
            .Map(d => d.Storage, s => s.Storage)
            .Map(d => d.StorageContentId, s => s.StorageContentId);
        //Остальные поля прописываются мануально
        TypeAdapterConfig<StorageContent, SaleContentDetail>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.StorageContentId, src => src.Id)
            .Map(dest => dest.Storage, src => src.StorageName)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime);

        //Остальные поля мануально прописываются BuyPriceInUsd, ArticleId, Status, Id
        TypeAdapterConfig<SaleContentDetailDto, StorageContent>.NewConfig()
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
            .Map(dest => dest.Query, src => JsonSerializer.Serialize(src.Query, Global.JsonOptions))
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
            .Map(dest => dest.GivenPrice,
                src => src.GivenPrice == null ? (decimal?)null : Math.Round(src.GivenPrice.Value, 2))
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

        TypeAdapterConfig<MarkupRange, MarkupRangeDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Markup, src => src.Markup)
            .Map(dest => dest.RangeEnd, src => src.RangeEnd)
            .Map(dest => dest.RangeStart, src => src.RangeStart);

        TypeAdapterConfig<NewMarkupRangeDto, MarkupRange>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Markup, src => src.Markup / 100)
            .Map(dest => dest.RangeEnd, src => src.RangeEnd)
            .Map(dest => dest.RangeStart, src => src.RangeStart);

        //CURRENCY
        TypeAdapterConfig<CreateCurrencyCommand, Currency>.NewConfig()
            .Map(x => x.Code, src => src.Code.Trim())
            .Map(x => x.Name, src => src.Name.Trim())
            .Map(x => x.CurrencySign, src => src.CurrencySign.Trim())
            .Map(x => x.ShortName, src => src.ShortName.Trim());

        TypeAdapterConfig<Currency, global::Contracts.Models.Currency.Currency>.NewConfig()
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ShortName, s => s.ShortName)
            .Map(d => d.CurrencySign, s => s.CurrencySign);


        //Emails
        TypeAdapterConfig<EmailDto, UserEmail>.NewConfig()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.NormalizedEmail, src => src.Email.ToNormalizedEmail())
            .Map(dest => dest.IsPrimary, src => src.IsPrimary)
            .Map(dest => dest.EmailType, src => src.Type.ToString())
            .Map(dest => dest.Confirmed, src => src.IsConfirmed)
            .Map(dest => dest.ConfirmedAt, src => src.IsConfirmed ? DateTime.UtcNow : (DateTime?)null);
        
        TypeAdapterConfig<UserEmail, FullEmailDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.IsPrimary, src => src.IsPrimary)
            .Map(dest => dest.EmailType, src => src.EmailType)
            .Map(dest => dest.Confirmed, src => src.Confirmed)
            .Map(dest => dest.ConfirmedAt, src => src.ConfirmedAt)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        TypeAdapterConfig<Permission, PermissionDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Description, s => s.Description);

        TypeAdapterConfig<Cart, CartItemDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.Article, s => s.Article);
        
        
        //Storage Route
        
        TypeAdapterConfig<AddStorageRouteCommand, StorageRoute>.NewConfig()
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.FromStorageName, s => s.StorageFrom)
            .Map(d => d.ToStorageName, s => s.StorageTo)
            .Map(d => d.DistanceM, s => s.Distance)
            .Map(d => d.RouteType, s => s.RouteType)
            .Map(d => d.PricingModel, s => s.PricingType)
            .Map(d => d.DeliveryTimeMinutes, s => s.DeliveryTime)
            .Map(d => d.PriceKg, s => s.PriceKg)
            .Map(d => d.PricePerM3, s => s.PriceM3)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder)
            .Map(d => d.IsActive, s => false)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice)
            .Map(d => d.CarrierId, s => s.CarrierId);
        
        TypeAdapterConfig<StorageRoute, StorageRouteDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.FromStorageName, s => s.FromStorageName)
            .Map(d => d.ToStorageName, s => s.ToStorageName)
            .Map(d => d.DistanceM, s => s.DistanceM)
            .Map(d => d.RouteType, s => s.RouteType)
            .Map(d => d.PricingModel, s => s.PricingModel)
            .Map(d => d.DeliveryTimeMinutes, s => s.DeliveryTimeMinutes)
            .Map(d => d.PricePerKg, s => s.PriceKg)
            .Map(d => d.PricePerM3, s => s.PricePerM3)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder)
            .Map(d => d.IsActive, s => s.IsActive)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.CurrencySign, s => s.Currency.CurrencySign)
            .Map(d => d.CurrencyName, s => s.Currency.Name)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice)
            .Map(d => d.CarrierId, s => s.CarrierId);
        
        TypeAdapterConfig<PatchStorageRouteDto, StorageRoute>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.DistanceM, s => s.DistanceM.Value)
            .Map(d => d.RouteType, s => s.RouteType.Value)
            .Map(d => d.PricingModel, s => s.PricingModel.Value)
            .Map(d => d.DeliveryTimeMinutes, s => s.DeliveryTimeMinutes.Value)
            .Map(d => d.PriceKg, s => s.PriceKg.Value)
            .Map(d => d.PricePerM3, s => s.PricePerM3.Value)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder.Value)
            .Map(d => d.IsActive, s => s.IsActive.Value)
            .Map(d => d.CurrencyId, s => s.CurrencyId.Value)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice.Value)
            .Map(d => d.CarrierId, s => s.CarrierId.Value);
        
        //Article weight
        TypeAdapterConfig<ArticleWeight, ArticleWeightDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Weight, s => s.Weight)
            .Map(d => d.Unit, s => s.Unit);
        
        //Article size
        TypeAdapterConfig<ArticleSize, ArticleSizeDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Height, s => s.Height)
            .Map(d => d.Width, s => s.Width)
            .Map(d => d.Length, s => s.Length)
            .Map(d => d.Unit, s => s.Unit)
            .Map(d => d.VolumeM3, s => s.VolumeM3);
        
        //Logistics
        TypeAdapterConfig<LogisticsCalcItemResult, DeliveryCostItemDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.Id)
            .Map(d => d.Quantity, s => s.Quantity)
            .Map(d => d.Weight, s => s.Weight)
            .Map(d => d.WeightPerItem, s => s.WeightPerItem)
            .Map(d => d.WeightUnit, s => s.WeightUnit)
            .Map(d => d.AreaM3, s => s.AreaM3)
            .Map(d => d.AreaPerItem, s => s.AreaPerItem)
            .Map(d => d.Cost, s => s.Cost)
            .Map(d => d.Skipped, s => s.Skipped)
            .Map(d => d.Reasons, s => s.Reasons);
        
        TypeAdapterConfig<LogisticsCalcResult, DeliveryCostDto>.NewConfig()
            .Map(d => d.WeightUnit, s => s.WeightUnit)
            .Map(d => d.TotalWeight, s => s.TotalWeight)
            .Map(d => d.TotalAreaM3, s => s.TotalAreaM3)
            .Map(d => d.Items, s => s.Items)
            .Map(d => d.TotalCost, s => s.TotalCost)
            .Map(d => d.MinimalPrice, s => s.MinimalPrice)
            .Map(d => d.MinimalPriceApplied, s => s.MinimalPriceApplied)
            .Map(d => d.PricingModel, s=> s.PricingModel);
        
        TypeAdapterConfig<ArticleCoefficient, PriceCoefficient>.NewConfig()
            .Map(d => d.Value, s => s.CoefficientNameNavigation.Value)
            .Map(d => d.Name, s => s.CoefficientName)
            .Map(d => d.Order, x => x.CoefficientNameNavigation.Order)
            .Map(d => d.Type, x => x.CoefficientNameNavigation.Type)
            .Map(d => d.ValidTill, s => s.ValidTill);
    }
}