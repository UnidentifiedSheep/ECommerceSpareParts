using Main.Application.Dtos.Sale;
using Main.Application.Models.Sale;
using Main.Entities.Sale;

namespace Main.Application.Interfaces.Services;

public interface ISaleService
{
    List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<NewSaleContentDto> saleContents);

    List<SaleContent> DistributeDetails(
        IEnumerable<StorageLot> storageContentValues,
        IEnumerable<EditSaleContentDto> saleContents);
}