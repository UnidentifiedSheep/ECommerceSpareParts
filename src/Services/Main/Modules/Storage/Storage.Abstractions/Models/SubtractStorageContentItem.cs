namespace Main.Modules.Storage.Abstractions.Models;

public record SubtractStorageContentItem(int StorageContentId, int Count);

public record SubtractProductFromStorageItem(int ProductId, string StorageCode, int Count);
