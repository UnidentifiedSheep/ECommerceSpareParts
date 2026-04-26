using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.Models;
using Contracts.Models.StorageContent;
using Extensions;
using Main.Abstractions.Models;
using Main.Abstractions.Models.Logistics;
using Main.Application.Configs.Mapster.ContractMappings;
using Main.Application.Dtos.Amw.ArticleCharacteristics;
using Main.Application.Dtos.Amw.ArticleCoefficients;
using Main.Application.Dtos.Amw.Balances;
using Main.Application.Dtos.Amw.Coefficients;
using Main.Application.Dtos.Amw.Logistics;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Amw.Sales;
using Main.Application.Dtos.Amw.Storage;
using Main.Application.Dtos.Auth;
using Main.Application.Dtos.Cart;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Member.Vehicles;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Sale;
using Main.Application.Dtos.Storage;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Handlers.ArticlePairs.CreatePair;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Entities;
using Main.Entities.Auth;
using Main.Entities.Balance;
using Main.Entities.Cart;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Entities.User;
using Mapster;
using Utils;
using MemberArticleFullDto = Main.Application.Dtos.Member.Articles.ArticleFullDto;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Application.Dtos.Anonymous.Articles.ArticleDto;
using AmwPurchaseDto = Main.Application.Dtos.Amw.Purchase.PurchaseDto;
using MemberPurchaseDto = Main.Application.Dtos.Member.Purchase.PurchaseDto;
using Currency = Main.Entities.Currency.Currency;
using ContractArticle = Contracts.Models.Articles.Article;
using ArticleCoefficientContract = Contracts.Models.ArticleCoefficients.ArticleCoefficient;
using CoefficientContract = Contracts.Models.Coefficients.Coefficient;

namespace Main.Application.Configs.Mapster;

public static class MapsterConfig
{
    public static void Configure()
    {
        MapsterUserConfig.Configure();
        PurchaseContractConfig.Configure();
        TypeAdapterConfig<Product, ContractArticle>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.Sku)
            .Map(d => d.NormalizedArticleNumber, s => s.NormalizedSku)
            .Map(d => d.ArticleName, s => s.Name)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.PackingUnit, s => s.PackingUnit)
            .Map(d => d.TotalCount, s => s.Stock)
            .Map(d => d.Indicator, s => s.Indicator)
            .Map(d => d.CategoryId, s => s.CategoryId)
            .Map(d => d.Popularity, s => s.Popularity);

        TypeAdapterConfig<ProductCoefficientDto, ArticleCoefficientContract>.NewConfig()
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.ValidTill, s => s.ValidTill)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.Coefficient, s => s.Coefficient);

        TypeAdapterConfig<Coefficient, CoefficientContract>.NewConfig()
            .Map(d => d.Value, s => s.Value)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Order, s => s.Order)
            .Map(d => d.Type, s => s.Type);

        TypeAdapterConfig<ProductCoefficient, ProductCoefficientDto>.NewConfig()
            .Map(d => d.ArticleId, s => s.ProductId)
            .Map(d => d.ValidTill, s => s.ValidTill)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.Coefficient, s => s.Coefficient);

        TypeAdapterConfig<Coefficient, CoefficientDto>.NewConfig()
            .Map(d => d.Value, s => s.Value)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Order, s => s.Order)
            .Map(d => d.Type, s => s.Type);

        TypeAdapterConfig<StorageContentPriceProjection, StorageContentCost>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.ArticleId, s => s.ProductId)
            .Map(d => d.PurchaseId, s => s.PurchaseId)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.StorageContentId, s => s.StorageContentId)
            .Map(d => d.PurchaseContentId, s => s.PurchaseContentId)
            .Map(d => d.CurrentCount, s => s.CurrentCount)
            .Map(d => d.DeliveryPrice, s =>
                s.LogisticsPrice != null && s.PurchaseContentCount != null
                    ? s.LogisticsPrice / s.PurchaseContentCount
                    : 0)
            .Map(d => d.DeliveryCurrencyId, s =>
                s.LogisticsPrice != null && s.PurchaseContentCount != null
                    ? s.LogisticsCurrencyId
                    : s.CurrencyId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.PurchaseContentCount, s => s.PurchaseContentCount)
            .Map(d => d.PurchaseDatetime, s => s.PurchaseDatetime);

        TypeAdapterConfig<PurchaseContent, PurchaseContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Article, src => src.Product)
            .Map(dest => dest.TotalSum, src => src.TotalSum)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Count, src => src.Count);

        TypeAdapterConfig<NewPurchaseContentDto, PurchaseContent>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.TotalSum, src => src.Price * src.Count)
            .Map(dest => dest.Comment, src => src.Comment);

        TypeAdapterConfig<NewPurchaseContentDto, NewStorageContentDto>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ArticleId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.BuyPrice, src => src.Price);

        TypeAdapterConfig<NewPurchaseContentDto, LogisticsItemDto>.NewConfig()
            .Map(d => d.ProductId, s => s.ArticleId)
            .Map(d => d.Quantity, s => s.Count);

        TypeAdapterConfig<EditPurchaseDto, LogisticsItemDto>.NewConfig()
            .Map(d => d.ProductId, s => s.ArticleId)
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
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.PurchaseDatetime,
                src => src.PurchaseDate);

        TypeAdapterConfig<NewCharacteristicsDto, ProductCharacteristic>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Name, src => src.Name == null ? null : src.Name.Trim())
            .Map(dest => dest.Value, src => src.Value.Trim());
        //Articles
        //ALL
        TypeAdapterConfig<Product, AmwArticleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProducerId, src => src.ProducerId)
            .Map(dest => dest.ProducerName, src => src.Producer.Name)
            .Map(dest => dest.Title, src => src.Name)
            .Map(dest => dest.ArticleNumber, src => src.Sku)
            .Map(dest => dest.CurrentStock, src => src.Stock)
            .Map(dest => dest.Indicator, src => src.Indicator);

        TypeAdapterConfig<Product, AnonymousArticleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProducerName, src => src.Producer.Name)
            .Map(dest => dest.Title, src => src.Name)
            .Map(dest => dest.ArticleNumber, src => src.Sku)
            .Map(dest => dest.CurrentStock, src => src.Stock);


        //AMW
        TypeAdapterConfig<CreateProductDto, Product>.NewConfig()
            .Map(d => d.Name, s => s.Name.Trim())
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim())
            .Map(d => d.Sku, s => s.Sku.Trim())
            .Map(d => d.NormalizedSku, s => s.Sku.ToNormalizedArticleNumber())
            .Map(d => d.PackingUnit, s => s.PackingUnit)
            .Map(d => d.Indicator, s => string.IsNullOrWhiteSpace(s.Indicator) ? null : s.Indicator.Trim())
            .Map(d => d.CategoryId, s => s.CategoryId);
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.Sku)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.Producer.Name)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.Images.Select(x => x.Path))
            .Map(d => d.Stock, s => s.Stock)
            .Map(d => d.Indicator, s => s.Indicator);


        TypeAdapterConfig<ProductDto, MemberArticleFullDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.ArticleNumber)
            .Map(d => d.Title, s => s.Name)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.ProducerName)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.Images)
            .Map(d => d.CurrentStock, s => s.Stock);

        TypeAdapterConfig<PatchCharacteristicsDto, ProductCharacteristic>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.Name, s => s.Name.Value == null ? null : s.Name.Value.Trim())
            .Map(d => d.Value, s => s.Value.Value == null ? null : s.Value.Value.Trim());

        TypeAdapterConfig<CreateStorageCommand, Storage>.NewConfig()
            .Map(d => d.Name, s => s.Name.Trim())
            .Map(d => d.Description, s => s.Description == null ? null : s.Description.Trim())
            .Map(d => d.Location, s => s.Location == null ? null : s.Location.Trim())
            .Map(d => d.Type, s => s.Type.ToString());

        TypeAdapterConfig<PatchProductDto, Product>.NewConfig()
            .IgnorePatchIfNotSet()
            .IgnoreIf((src, dest) => !src.Sku.IsSet, dest => dest.NormalizedSku)
            .Map(d => d.Name, s => s.Name.Value)
            .Map(d => d.ProducerId, s => s.ProducerId.Value)
            .Map(d => d.Description, s => s.Description.Value)
            .Map(d => d.Sku, s => s.Sku.Value)
            .Map(d => d.NormalizedSku, s => s.Sku.Value!.ToNormalizedArticleNumber())
            .Map(d => d.PackingUnit, s => s.PackingUnit.Value)
            .Map(d => d.Indicator, s => s.Indicator.Value)
            .Map(d => d.CategoryId, s => s.CategoryId.Value);

        //MEMBER
        TypeAdapterConfig<Product, MemberArticleFullDto>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleNumber, s => s.Sku)
            .Map(d => d.Title, s => s.Name)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.ProducerName, s => s.Producer.Name)
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.Images, s => s.Images.Select(x => x.Path))
            .Map(d => d.CurrentStock, s => s.Stock);

        //Producers
        TypeAdapterConfig<Producer, ProducerDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.IsOe, src => src.IsOe)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
        TypeAdapterConfig<NewProducerDto, Producer>.NewConfig()
            .Map(d => d.Name, s => s.Name.ToNormalized())
            .Map(d => d.IsOe, s => s.IsOe)
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim());
        TypeAdapterConfig<ProducerOtherName, ProducerOtherNameDto>.NewConfig()
            .Map(d => d.ProducerId, s => s.ProducerId)
            .Map(d => d.OtherName, s => s.OtherName)
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
            .Map(dest => dest.TotalSum, src => src.Transaction.Amount)
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
            .Map(d => d.ProductId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.TotalSum, s => s.Price * s.Count);

        TypeAdapterConfig<PurchaseLogistic, PurchaseLogisticDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.PurchaseId, s => s.PurchaseId)
            .Map(d => d.Currency, s => s.Currency)
            .Map(d => d.MinimumPrice, s => s.MinimumPrice)
            .Map(d => d.MinimumPriceApplied, s => s.MinimumPriceApplied)
            .Map(d => d.PriceKg, s => s.PriceKg)
            .Map(d => d.PricePerM3, s => s.PricePerM3)
            .Map(d => d.PricePerOrder, s => s.PricePerOrder)
            .Map(d => d.PricingModel, s => s.PricingModel)
            .Map(d => d.RouteId, s => s.RouteId)
            .Map(d => d.RouteType, s => s.RouteType)
            .Map(d => d.TransactionId, s => s.TransactionId);
        
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
            .Map(dest => dest.TotalSum, src => src.Transaction.Amount)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.PurchaseDatetime, src => src.SaleDatetime);


        //Roles

        TypeAdapterConfig<Role, RoleDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
        //Balances

        TypeAdapterConfig<Transaction, TransactionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SenderId, s => s.SenderId)
            .Map(dest => dest.ReceiverId, s => s.ReceiverId)
            .Map(dest => dest.Amount, s => s.Amount)
            .Map(dest => dest.Status, s => s.Type)
            .Map(dest => dest.TransactionDate, dest => dest.TransactionDatetime)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId);

        TypeAdapterConfig<Transaction, TransactionVersion>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(dest => dest.TransactionId, src => src.Id)
            .Map(dest => dest.SenderId, s => s.SenderId)
            .Map(dest => dest.ReceiverId, s => s.ReceiverId)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId)
            .Map(dest => dest.Status, s => s.Type)
            .Map(dest => dest.TransactionDatetime,
                dest => dest.TransactionDatetime)
            .Map(dest => dest.TransactionSum, dest => dest.Amount);

        //STORAGES

        TypeAdapterConfig<StorageContent, StorageMovement>.NewConfig()
            .Ignore(x => x.Id)
            .Map(dest => dest.CurrencyId, src => src.CurrencyId)
            .Map(dest => dest.Price, s => s.BuyPrice)
            .Map(dest => dest.Count, s => s.Count)
            .Map(dest => dest.StorageName, s => s.StorageName)
            .Map(dest => dest.ProductId, s => s.ProductId);

        TypeAdapterConfig<StorageContent, StorageContent>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.StorageName, s => s.StorageName)
            .Map(dest => dest.ProductId, s => s.ProductId)
            .Map(dest => dest.CurrencyId, s => s.CurrencyId)
            .Map(dest => dest.Count, s => s.Count)
            .Map(dest => dest.BuyPrice, s => s.BuyPrice)
            .Map(dest => dest.BuyPriceInUsd, s => s.BuyPriceInUsd)
            .Map(dest => dest.PurchaseDatetime, s => s.PurchaseDatetime);

        TypeAdapterConfig<PatchStorageDto, Storage>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(d => d.Location, s => string.IsNullOrWhiteSpace(s.Location.Value) ? null : s.Location.Value.Trim())
            .Map(d => d.Description,
                s => string.IsNullOrWhiteSpace(s.Description.Value) ? null : s.Description.Value.Trim())
            .Map(d => d.Type, s => s.Type.Value);

        TypeAdapterConfig<StorageContent, StorageContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.BuyPrice, src => src.BuyPrice)
            .Map(dest => dest.StorageName, src => src.StorageName)
            .Map(dest => dest.PurchaseDatetime, src => src.PurchaseDatetime)
            .Map(dest => dest.ConcurrencyCode, src =>
                HashUtils.ComputeHash(src.Id, src.ProductId, src.BuyPrice, src.CurrencyId,
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
            .Map(dest => dest.TotalSum, src => src.Transaction.Amount)
            .Map(dest => dest.TransactionId, src => src.TransactionId)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.SaleDatetime, src => src.SaleDatetime)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Storage, src => src.StorageName);

        TypeAdapterConfig<SaleContent, SaleContentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(d => d.Product, s => s.Product)
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
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.PriceWithDiscount)
            .Map(dest => dest.TotalSum, src => src.PriceWithDiscount * src.Count)
            .Map(dest => dest.Discount,
                src => Price.GetDiscountFromPrices(src.PriceWithDiscount, src.Price));

        TypeAdapterConfig<EditSaleContentDto, SaleContent>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
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
            .Map(d => d.CreationDatetime, s => s.CreatedAt)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.MainStorageName, s => s.StorageName)
            .Map(d => d.SaleContents, s => s.SaleContents)
            .Map(d => d.SaleDatetime, s => s.SaleDatetime)
            .Map(d => d.TransactionId, s => s.TransactionId);

        TypeAdapterConfig<SaleContent, Contracts.Models.Sale.SaleContent>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleId, s => s.ProductId)
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
        //User Search History

        TypeAdapterConfig<SearchLogModel, UserSearchHistory>.NewConfig()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Query, src => JsonSerializer.Serialize(src.Query, Global.JsonOptions))
            .Map(dest => dest.SearchDateTime, src => src.SearchDateTime)
            .Map(dest => dest.SearchPlace, src => src.SearchPlace);

        //Article Reservation 
        TypeAdapterConfig<EditProductReservationDto, StorageContentReservation>.NewConfig()
            .IgnorePatchIfNotSet()
            .Map(dest => dest.ProductId, src => src.ArticleId)
            .Map(dest => dest.Comment, src => src.Comment == null ? null : src.Comment.Trim())
            .Map(dest => dest.ReservedCount, src => src.InitialCount)
            .Map(dest => dest.CurrentCount, src => src.CurrentCount)
            .Map(dest => dest.ProposedCurrencyId, src => src.GivenCurrencyId)
            .Map(dest => dest.ProposedPrice, src => src.GivenPrice);

        TypeAdapterConfig<NewProductReservationDto, StorageContentReservation>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.ProposedCurrencyId, src => src.GivenCurrencyId)
            .Map(dest => dest.ProposedPrice,
                src => src.ProposedPrice == null ? (decimal?)null : Math.Round(src.ProposedPrice.Value, 2))
            .Map(dest => dest.CurrentCount, src => src.CurrentCount)
            .Map(dest => dest.ReservedCount, src => src.ReservedCount)
            .Map(dest => dest.UserId, src => src.UserId);

        //CURRENCY
        TypeAdapterConfig<CreateCurrencyCommand, Currency>.NewConfig()
            .Map(x => x.Code, src => src.Code.Trim())
            .Map(x => x.Name, src => src.Name.Trim())
            .Map(x => x.CurrencySign, src => src.CurrencySign.Trim())
            .Map(x => x.ShortName, src => src.ShortName.Trim());

        TypeAdapterConfig<Currency, Contracts.Models.Currency.Currency>.NewConfig()
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ShortName, s => s.ShortName)
            .Map(d => d.CurrencySign, s => s.CurrencySign)
            .Map(d => d.ToUsdRate, s => s.CurrencyToUsd == null ? 0 : s.CurrencyToUsd.ToUsd);

        TypeAdapterConfig<Currency, CurrencyDto>.NewConfig()
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ShortName, s => s.ShortName)
            .Map(d => d.CurrencySign, s => s.CurrencySign)
            .Map(d => d.ToUsdRate, s => s.CurrencyToUsd == null ? 0 : s.CurrencyToUsd.ToUsd);

        TypeAdapterConfig<CurrencyDto, Contracts.Models.Currency.Currency>.NewConfig()
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ShortName, s => s.ShortName)
            .Map(d => d.CurrencySign, s => s.CurrencySign)
            .Map(d => d.ToUsdRate, s => s.ToUsdRate);

        //Emails
        TypeAdapterConfig<EmailDto, UserEmail>.NewConfig()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.NormalizedEmail, src => src.Email.ToNormalizedEmail())
            .Map(dest => dest.IsPrimary, src => src.IsPrimary)
            .Map(dest => dest.EmailType, src => src.Type.ToString())
            .Map(dest => dest.Confirmed, src => src.IsConfirmed)
            .Map(dest => dest.ConfirmedAt, src => src.IsConfirmed ? DateTime.UtcNow : (DateTime?)null);

        TypeAdapterConfig<UserEmail, UserEmailDto>.NewConfig()
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
            .Map(d => d.ProductId, s => s.ProductId)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.Product, s => s.Product);


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
        TypeAdapterConfig<ProductWeight, ProductWeightDto>.NewConfig()
            .Map(d => d.ProductId, s => s.ProductId)
            .Map(d => d.Weight, s => s.Weight)
            .Map(d => d.Unit, s => s.Unit);

        //Article size
        TypeAdapterConfig<ProductSize, ProductSizeDto>.NewConfig()
            .Map(d => d.ProductId, s => s.ProductId)
            .Map(d => d.Height, s => s.Height)
            .Map(d => d.Width, s => s.Width)
            .Map(d => d.Length, s => s.Length)
            .Map(d => d.Unit, s => s.Unit)
            .Map(d => d.VolumeM3, s => s.VolumeM3);

        //Logistics
        TypeAdapterConfig<LogisticsCalcItemResult, DeliveryCostItemDto>.NewConfig()
            .Map(d => d.ProductId, s => s.Id)
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
            .Map(d => d.PricingModel, s => s.PricingModel);
    }
}