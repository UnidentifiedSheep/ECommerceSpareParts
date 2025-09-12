using Core.Enums;

namespace Core.Dtos.Services.StorageContents;

public record AddContentToStorageDto(
    IEnumerable<(int ArticleId, int Count, decimal Price, int currencyId)> Content,
    string StorageName,
    string UserId,
    StorageMovementType MovementType);