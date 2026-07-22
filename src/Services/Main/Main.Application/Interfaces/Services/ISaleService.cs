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

    Task CheckReservations(
        IEnumerable<NewSaleContentDto> saleContents,
        Guid buyerOrganizationId,
        string storageName,
        bool takeFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken = default);

    Task CheckReservations(
        IEnumerable<EditSaleContentDto> saleContents,
        Guid buyerOrganizationId,
        string storageName,
        bool takeFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
        string storageName,
        IEnumerable<NewSaleContentDto> saleContents,
        StorageMovementType movementType,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleContent>> TakeFromStorageAndDistributeDetails(
        string storageName,
        IEnumerable<EditSaleContentDto> saleContents,
        StorageMovementType movementType,
        bool takeFromOtherStorages,
        CancellationToken cancellationToken = default);

    Task UpdateOrganizationReservationCounts(
        Guid buyerOrganizationId,
        Dictionary<int, int> counts,
        CancellationToken cancellationToken = default);

    Task ConsumeOrganizationReservations(
        Sale sale,
        Guid buyerOrganizationId,
        CancellationToken cancellationToken = default);

    Task RestoreContents(
        Sale sale,
        StorageMovementType movementType,
        CancellationToken cancellationToken = default);
}
