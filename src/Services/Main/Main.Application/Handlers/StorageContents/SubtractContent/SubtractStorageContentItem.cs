namespace Main.Application.Handlers.StorageContents.SubtractContent;

public interface ISubtractStorageContentItem
{
    int Count { get; }
}

public record SubtractStorageContentItem(
    int StorageContentId, 
    int Count) : ISubtractStorageContentItem;

public record SubtractProductFromStorageItem(
    int ProductId, 
    string StorageName, 
    int Count) : ISubtractStorageContentItem;