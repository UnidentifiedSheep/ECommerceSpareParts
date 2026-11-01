using Main.Application.Dtos.Sale;
using Main.Application.Models.Storage;
using Main.Entities.Sale;
using Main.Enums;

namespace Main.Application.Interfaces.Services;

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
		bool takeFromOtherStorages,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
		string storageCode,
		IEnumerable<EditSaleContentDto> saleContents,
		StorageMovementType movementType,
		bool takeFromOtherStorages,
		CancellationToken cancellationToken = default);

	Task RestoreContents(
		Sale sale,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default);
}
