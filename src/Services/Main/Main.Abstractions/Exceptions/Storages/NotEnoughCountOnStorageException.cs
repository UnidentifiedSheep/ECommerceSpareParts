using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Storages;

public class NotEnoughCountOnStorageException : BadRequestException, ILocalizableException
{
    public string MessageKey { get; }
    public object[]? Arguments { get; }
    public NotEnoughCountOnStorageException(int articleId, int availableCount, int neededCount)
        : base(null, new { ArticleId = articleId, AvailableCount = availableCount })
    {
        MessageKey = "not.enough.count.on.storage.for.article";
        Arguments = [articleId, availableCount, neededCount];
    }
    
    public NotEnoughCountOnStorageException(IEnumerable<int> ids) 
        : base(null, new { Ids = ids })
    {
        MessageKey = "not.enough.count.on.storage.for.articles";
        Arguments = null;
    }
}