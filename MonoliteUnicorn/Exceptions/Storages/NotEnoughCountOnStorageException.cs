using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class NotEnoughCountOnStorageException : BadRequestException
{
    public NotEnoughCountOnStorageException(string storageName, int articleId, int availableCount) : 
        base($"Нет достаточного количества на складе '{storageName}' для {articleId}.\n Всего доступно {availableCount}", 
            new { StorageName = storageName, ArticleId = articleId, AvailableCount = availableCount })
    {
        
    }

    public NotEnoughCountOnStorageException(int articleId, int availableCount) 
        : base($"Нет достаточного количества на складе для {articleId}.\n Всего доступно {availableCount}", new { ArticleId = articleId, AvailableCount = availableCount })
    {
    }
    
    public NotEnoughCountOnStorageException(IEnumerable<int> ids) : base($"Нет достаточного количества на складе для артикулов.", new { Ids = ids })
    {
    }
    
}
