using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace Search.Persistence.HealthChecks;

public sealed class OpenSearchHealthCheck(IOpenSearchClient client) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		var response = await client.PingAsync(request => request, cancellationToken);
		if (response.IsValid)
			return HealthCheckResult.Healthy();

		var description = response.ServerError?.Error?.Reason ?? response.OriginalException?.Message;
		return new HealthCheckResult(
			context.Registration.FailureStatus,
			description ?? "OpenSearch ping failed.",
			response.OriginalException);
	}
}
