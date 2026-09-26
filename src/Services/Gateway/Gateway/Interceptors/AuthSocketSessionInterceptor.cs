using System.Text.Json;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.AspNetCore.Subscriptions.Protocols;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authentication;
using Security;

namespace Gateway.Interceptors;

public sealed class AuthSocketSessionInterceptor : DefaultSocketSessionInterceptor
{
	public override async ValueTask<ConnectionStatus> OnConnectAsync(
		ISocketSession session,
		IOperationMessagePayload connectionInitMessage,
		CancellationToken cancellationToken = default)
	{
		if (connectionInitMessage.Payload is not
			{ ValueKind: JsonValueKind.Object } payload)
			return ConnectionStatus.Reject("Authorization payload is missing.");

		if (!payload.TryGetProperty("authorization", out var authorization) ||
			authorization.ValueKind != JsonValueKind.String)
			return ConnectionStatus.Reject("Authorization token is missing.");

		var value = authorization.GetString();

		if (string.IsNullOrWhiteSpace(value))
			return ConnectionStatus.Reject("Authorization token is missing.");

		var httpContext = session.Connection.HttpContext;


		httpContext.Request.Headers.Authorization = value;

		//non default schema used cuz, default already worked and result is chached.
		var result = await httpContext.AuthenticateAsync(AuthenticationSchemes.WebSocketBearer);

		if (!result.Succeeded || result.Principal is null)
			return ConnectionStatus.Reject("Unauthorized.");

		httpContext.User = result.Principal;

		return await base.OnConnectAsync(
			session,
			connectionInitMessage,
			cancellationToken);
	}

	public override async ValueTask OnRequestAsync(
		ISocketSession session,
		string operationSessionId,
		OperationRequestBuilder requestBuilder,
		CancellationToken cancellationToken = default)
	{
		await base.OnRequestAsync(
			session,
			operationSessionId,
			requestBuilder,
			cancellationToken);
	}
}
