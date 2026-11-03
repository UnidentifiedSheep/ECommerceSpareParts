using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Application.Extensions;

public static class StorageContentRepositoryExtensions
{
	public static async Task<StorageContent> GetWithActiveLocksForUpdateAsync(
		this IRepository<StorageContent, int> repository,
		int contentId,
		CriteriaBuilder<StorageContent>? criteriaBuilder = null,
		CancellationToken cancellationToken = default)
	{
		var contents = await repository
			.GetWithActiveLocksForUpdateAsync([contentId], criteriaBuilder, cancellationToken);
		return contents[contentId];
	}

	public static async Task<Dictionary<int, StorageContent>> GetWithActiveLocksForUpdateAsync(
		this IRepository<StorageContent, int> repository,
		IReadOnlyCollection<int> contentIds,
		CriteriaBuilder<StorageContent>? criteriaBuilder = null,
		CancellationToken cancellationToken = default)
	{
		if (contentIds.Count == 0) return [];

		var contents = await repository.GetWithActiveLocksForUpdateAsync(
			(criteriaBuilder ?? Criteria<StorageContent>.New()).Where(content => contentIds.Contains(content.Id)),
			cancellationToken);
		var contentsById = contents.ToDictionary(content => content.Id);
		contentIds.EnsureAllExists(contentsById.Keys, missing => new StorageContentNotFoundException(missing));
		return contentsById;
	}

	public static async Task<List<StorageContent>> GetWithActiveLocksForUpdateAsync(
		this IRepository<StorageContent, int> repository,
		CriteriaBuilder<StorageContent> criteriaBuilder,
		CancellationToken cancellationToken = default)
	{
		var contents = await repository.ListAsync(
			criteriaBuilder
				.OrderByAsc(content => content.Id)
				.Track()
				.ForUpdate()
				.Build(),
			cancellationToken);
		await repository.LoadActiveLocksAsync(
			contents.Select(content => content.Id).ToArray(),
			cancellationToken);
		return contents;
	}

	// Acquire party locks before this query; it must see reservations committed while waiting.
	public static async Task LoadActiveLocksAsync(
		this IRepository<StorageContent, int> repository,
		IReadOnlyCollection<int> contentIds,
		CancellationToken cancellationToken = default)
	{
		if (contentIds.Count == 0) return;
		var contents = await repository.EnsureExistsAsync(
				ids: contentIds,
				errorFactory: m => new StorageContentNotFoundException(m),
				ct: cancellationToken,
				criteriaBuilder: Criteria<StorageContent>.New()
					.Include(content => content.Locks.Where(item => item.State == StorageContentLockState.Active))
					.Track());

		foreach (var (_, content) in contents)
			content.MarkLocksLoaded();
	}
}
