using Integrations.Common;
using Integrations.Favorit.Requests;
using Integrations.Favorit.Responses;

namespace Integrations.Favorit.Interfaces;

public interface IFavoritPartsClient
{
    Task<Response<GetPricesResponse>> GetPricesAsync(
        GetPricesRequest request,
        CancellationToken token = default);
}