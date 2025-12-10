using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class NotEnoughCountOnStorageException : BadRequestException
{
    [ExampleExceptionValues(false,"Пример_склад", 10, 0)]
    public NotEnoughCountOnStorageException(string storageName, int articleId, int availableCount) :
        base(
            $"Нет достаточного количества на складе '{storageName}' для {articleId}. Всего доступно {availableCount}",
            new { StorageName = storageName, ArticleId = articleId, AvailableCount = availableCount })
    {
    }
    [ExampleExceptionValues(false,10, 0)]

    public NotEnoughCountOnStorageException(int articleId, int availableCount)
        : base($"Нет достаточного количества на складе для {articleId}. Всего доступно {availableCount}",
            new { ArticleId = articleId, AvailableCount = availableCount })
    {
    }
    
    [ExampleExceptionValues(true,10, 123, 432)]

    public NotEnoughCountOnStorageException(IEnumerable<int> ids) : base(
        "Нет достаточного количества на складе для артикулов.", new { Ids = ids })
    {
    }
}