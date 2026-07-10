using Contracts.Models.Supplier;
using Integrations.Supplier.Models;

namespace Application.Common.Extensions;

public static class SupplierProductExtensions
{
    public static ContractSupplierProductDto ToContract(this SupplierProduct product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            Number = product.Number,
            Brand = product.Brand,
            
            Analogues = product.Analogues.Select(z => z.ToContract()).ToList(),
            Positions = product.Positions
                .Select(z => new ContractSupplierPositionDto
                {
                    Id = z.Id,
                    DeliveryInfo = z.DeliveryInfo == null 
                        ? null 
                        : new ContractDeliveryInfoDto
                        {
                            DeliveryDate = z.DeliveryInfo.DeliveryDate,
                            DeliveryProbability = z.DeliveryInfo.DeliveryProbability,
                            GuaranteedDeliveryDate = z.DeliveryInfo.GuaranteedDeliveryDate,
                            OrderTill = z.DeliveryInfo.OrderTill
                        },
                    PurchaseInfo = z.PurchaseInfo == null 
                        ? null 
                        : new ContractPurchaseInfoDto
                        {
                            AvailableQuantity = z.PurchaseInfo.AvailableQuantity,
                            DaysToRefund = z.PurchaseInfo.DaysToRefund,
                            MinimumPurchaseQuantity = z.PurchaseInfo.MinimumPurchaseQuantity,
                            PartnerWarehouse = z.PurchaseInfo.PartnerWarehouse,
                            PriceInfo = new ContractPriceInfoDto
                            {
                                CurrencyCode = z.PurchaseInfo.PriceInfo.CurrencyCode,
                                Price = z.PurchaseInfo.PriceInfo.Price
                            },
                            QuantityCoefficient = z.PurchaseInfo.QuantityCoefficient
                        }
                })
                .ToList()
        };
    
    public static SupplierProduct FromContract(this ContractSupplierProductDto product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            Number = product.Number,
            Brand = product.Brand,
            
            Analogues = product.Analogues.Select(z => z.FromContract()).ToList(),
            Positions = product.Positions
                .Select(z => new SupplierPosition
                {
                    Id = z.Id,
                    DeliveryInfo = z.DeliveryInfo == null 
                        ? null 
                        : new DeliveryInfo
                        {
                            DeliveryDate = z.DeliveryInfo.DeliveryDate,
                            DeliveryProbability = z.DeliveryInfo.DeliveryProbability,
                            GuaranteedDeliveryDate = z.DeliveryInfo.GuaranteedDeliveryDate,
                            OrderTill = z.DeliveryInfo.OrderTill
                        },
                    PurchaseInfo = z.PurchaseInfo == null 
                        ? null 
                        : new PurchaseInfo
                        {
                            AvailableQuantity = z.PurchaseInfo.AvailableQuantity,
                            DaysToRefund = z.PurchaseInfo.DaysToRefund,
                            MinimumPurchaseQuantity = z.PurchaseInfo.MinimumPurchaseQuantity,
                            PartnerWarehouse = z.PurchaseInfo.PartnerWarehouse,
                            PriceInfo = new PriceInfo
                            {
                                CurrencyCode = z.PurchaseInfo.PriceInfo.CurrencyCode,
                                Price = z.PurchaseInfo.PriceInfo.Price
                            },
                            QuantityCoefficient = z.PurchaseInfo.QuantityCoefficient
                        }
                })
                .ToList()
        };
}