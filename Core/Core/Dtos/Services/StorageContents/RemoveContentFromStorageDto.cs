using Core.Enums;

namespace Core.Dtos.Services.StorageContents;

public record RemoveContentFromStorageDto(
    IEnumerable<(int ArticleId, int Count)> Content,
    string UserId,
    string? StorageName,
    bool TakeFromOtherStorages,
    StorageMovementType MovementType);