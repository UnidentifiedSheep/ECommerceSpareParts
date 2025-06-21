using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class NotEnoughCountOnStorageException : BadRequestException
{
    public NotEnoughCountOnStorageException(string storageName, int articleId, int availableCount) : base($"Нет достаточного количества на складе '{storageName}' для {articleId}.\n Всего доступно {availableCount}")
    {
        
    }

    public NotEnoughCountOnStorageException(int articleId, int availableCount) : base($"Нет достаточного количества на складе для {articleId}.\n Всего доступно {availableCount}")
    {
    }
}
