using Enums;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Application.Dtos.Sale;
using Main.Modules.Storage.Abstractions.Models;

namespace Main.Modules.Sale.Application.Interfaces.Services;

public interface ISaleService
{
	List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<NewSaleContentDto> saleContents);

	List<SaleContent> DistributeDetails(
		IEnumerable<StorageLot> storageContentValues,
		IEnumerable<EditSaleContentDto> saleContents);

	Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
		string storageCode,
		IEnumerable<NewSaleContentDto> saleContents,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
		string storageCode,
		IEnumerable<EditSaleContentDto> saleContents,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);

	Task RestoreContents(
		Entities.Sale.Sale.Sale sale,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);
}
