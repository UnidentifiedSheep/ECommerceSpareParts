using Favorit.Integrations.Core.Requests;
using Favorit.Integrations.Core.Responses;
using Integrations.Common;

namespace Favorit.Integrations.Core.Interfaces;

public interface IFavoritPartsClient
{
    Task<Response<GetPricesResponse>> GetPricesAsync(
        GetPricesRequest request,
        CancellationToken token = default);
}