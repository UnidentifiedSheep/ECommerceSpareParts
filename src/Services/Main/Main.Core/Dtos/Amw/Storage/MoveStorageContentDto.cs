namespace Main.Core.Dtos.Amw.Storage;

public class MoveStorageContentDto
{
    public int StorageContentId { get; set; }
    public string NewStorageName { get; set; } = null!;
    public string ConcurrencyCode { get; set; } = null!;
}