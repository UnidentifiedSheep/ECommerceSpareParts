using System.Text.Json.Serialization;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class UserNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) 
    : InternalClientBase(authClient, optionsMonitor), IUserNode
{
    public async Task<Response<decimal>> GetUserDiscount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/users/{userId}/discount",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<GetUserDiscountResponse, decimal>(
            response,
            x => x.Discount,
            cancellationToken);
    }
    
    private record GetUserDiscountResponse
    {
        [JsonPropertyName("discount")]
        public decimal Discount { get; init; }
    }
}
