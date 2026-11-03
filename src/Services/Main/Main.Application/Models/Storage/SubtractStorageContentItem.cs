namespace Main.Application.Models.Storage;

public interface ISubtractStorageContentItem
{
	int Count { get; }
}

public record SubtractStorageContentItem(int StorageContentId, int Count) : ISubtractStorageContentItem;

public record SubtractProductFromStorageItem(
	int ProductId,
	string StorageCode,
	int Count) : ISubtractStorageContentItem;
