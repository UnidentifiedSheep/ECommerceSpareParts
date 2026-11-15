using Main.Entities.Storage;
using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class NotEnoughCountOnStorageException : LocalizedBadRequestException
{
	public NotEnoughCountOnStorageException(
		int articleId,
		int availableCount,
		int neededCount) : base(
		new NotEnoughCountOnStorageForArticleMessage()
			.WithArticleId(articleId)
			.WithAvailableCount(availableCount)
			.WithNeededCount(neededCount),
		new
		{
			ArticleId = articleId,
			AvailableCount = availableCount,
			NeededCount = neededCount
		})
	{
	}

	public NotEnoughCountOnStorageException(IEnumerable<int> ids) : base(
		NotEnoughCountOnStorageForArticlesMessage.Instance,
		new
		{
			Ids = ids
		})
	{
	}
}

public class StorageContentNotFoundException : LocalizedNotFoundException
{
	public StorageContentNotFoundException(int id) : base(
		StorageContentNotFoundMessage.Instance,
		new
		{
			Id = id
		})
	{
	}

	public StorageContentNotFoundException(IEnumerable<int> ids) : base(
		StorageContentNotFoundMessage.Instance,
		new
		{
			Ids = ids
		})
	{
	}
}

public class StorageNotFoundException(string code) : LocalizedNotFoundException(
	new StorageNotFoundMessage().WithCode(code),
	new
	{
		Code = code
	});

public class StorageCodeAlreadyTakenException(object values) : LocalizedConflictException(
	StorageCodeTakenMessage.Instance,
	values);
