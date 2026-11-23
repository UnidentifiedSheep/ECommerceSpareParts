using Application.Common.Interfaces;

namespace Application.Common.Extensions;

public static class DisplayExtensions
{
	public static async Task<TInfo?> GetAsync<TKey, TInfo>(
		this IDisplay<TKey, TInfo> display,
		TKey id,
		CancellationToken cancellationToken = default)
		where TKey : notnull
		where TInfo : class
		=> (await display.GetAsync([id], cancellationToken)).GetValueOrDefault(id);
}
