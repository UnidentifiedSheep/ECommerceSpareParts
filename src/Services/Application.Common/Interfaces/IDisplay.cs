namespace Application.Common.Interfaces;

public interface IDisplay<TKey, TInfo>
	where TKey : notnull
	where TInfo : class
{
	Task<IReadOnlyDictionary<TKey, TInfo>> GetAsync(
		IReadOnlyCollection<TKey> ids,
		CancellationToken cancellationToken = default);
}
