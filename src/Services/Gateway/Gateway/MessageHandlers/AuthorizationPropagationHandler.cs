namespace Gateway.MessageHandlers;

public sealed class AuthorizationPropagationHandler(
	IHttpContextAccessor httpContextAccessor)
	: DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (request.Headers.Authorization is not null)
			return base.SendAsync(request, cancellationToken);

		var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

		if (!string.IsNullOrWhiteSpace(authorization))
			request.Headers.TryAddWithoutValidation("Authorization", authorization);

		return base.SendAsync(request, cancellationToken);
	}
}
