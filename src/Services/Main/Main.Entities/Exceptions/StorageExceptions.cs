using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class ChangeOfStorageTypeRestrictedException()
	: LocalizedBadRequestException(StorageTypeChangeRestrictedMessage.Instance);

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
			ArticleId = articleId, AvailableCount = availableCount, NeededCount = neededCount
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

public class StorageOwnerNotFoundException(Guid userId, string storageCode) : LocalizedNotFoundException(
	new StorageNotFoundInUserMessage().WithStorageCode(storageCode),
	new
	{
		UserId = userId, StorageCode = storageCode
	});

public class StorageRouteActiveExistsException(string from, string to) : LocalizedConflictException(
	new ActiveStorageRouteExistsMessage().WithFrom(from).WithTo(to),
	new
	{
		From = from, To = to
	});

public class StorageRouteNotFound : LocalizedNotFoundException
{
	public StorageRouteNotFound(string storageFrom, string storageTo) : base(
		new StorageRouteNotFoundByNamesMessage()
			.WithStorageFrom(storageFrom)
			.WithStorageTo(storageTo),
		new
		{
			StorageFrom = storageFrom, StorageTo = storageTo
		})
	{
	}

	public StorageRouteNotFound(Guid id) : base(
		StorageRouteNotFoundByIdMessage.Instance,
		new
		{
			Id = id
		})
	{
	}
}
