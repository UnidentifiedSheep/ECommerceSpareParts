using Integrations.Armtek.Models;
using Integrations.Armtek.Requests;
using Integrations.Common;

namespace Integrations.Armtek.Interfaces;

public interface IArmtekClient
{
	Task<Response<IReadOnlyList<SearchItem>>> SearchAsync(
		SearchRequest request,
		CancellationToken token = default);

	Task<Response<IReadOnlyList<AssortmentItem>>> AssortmentSearchAsync(
		AssortmentSearchRequest request,
		CancellationToken token = default);
}
