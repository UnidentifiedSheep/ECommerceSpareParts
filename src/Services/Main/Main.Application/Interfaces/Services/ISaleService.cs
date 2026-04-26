using Main.Abstractions.Models;
using Main.Application.Dtos.Amw.Sales;
using Main.Application.Models.SaleService;
using Main.Entities;
using Main.Entities.Sale;
using Main.Entities.Storage;

namespace Main.Abstractions.Interfaces.Services;

public interface ISaleService
{
    List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<NewSaleContentDto> saleContents);

    List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<EditSaleContentDto> saleContents);
}