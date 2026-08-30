using System.Net.Http.Json;
using Abstractions.Models.Options;
using Integrations.Client.Core;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Integrations.Tmtr.Requests;
using Integrations.Tmtr.Responses;

namespace Integrations.Tmtr;

public interface ITmtrClient
{
	Task<Response<GetPricesResponse>> GetPricesAsync(
		GetPricesRequest request,
		CancellationToken token = default);

	Task<Response<GetProductsResponse>> GetProductsAsync(
		GetProductsRequest request,
		CancellationToken token = default);
}

public class TmtrClient(
	HttpClient client,
	IConnectionProvider<TmtrConnection> connectionProvider,
	ProjectJsonOptions jsonOptions) : ClientBase(jsonOptions), ITmtrClient
{
	public async Task<Response<GetPricesResponse>> GetPricesAsync(
		GetPricesRequest request,
		CancellationToken token = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Brand);

		var connection = await connectionProvider.GetConnectionAsync(token);
		using var httpRequest = CreateRequest(connection, "/API.asmx/Proboy");

		httpRequest.Content = JsonContent.Create(
			new
			{
				article = request.Number, brand = request.Brand
			});

		using var response = await client.SendAsync(httpRequest, token);
		return await ReadResponse<GetPricesResponse>(response, token);
	}
	public async Task<Response<GetProductsResponse>> GetProductsAsync(
		GetProductsRequest request,
		CancellationToken token = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);

		var connection = await connectionProvider.GetConnectionAsync(token);
		using var httpRequest = CreateRequest(connection, "/API.asmx/PreProboy");

		httpRequest.Content = JsonContent.Create(
			new
			{
				article = request.Number
			});

		using var response = await client.SendAsync(httpRequest, token);
		return await ReadResponse<GetProductsResponse>(response, token);
	}

	private HttpRequestMessage CreateRequest(TmtrConnection connection, string relativeUrl)
	{
		var httpRequest = new HttpRequestMessage
		{
			Method = HttpMethod.Post, RequestUri = new Uri(new Uri(connection.BaseUrl), relativeUrl)
		};

		AddAuthHeaders(httpRequest, connection);
		return httpRequest;
	}

	private void AddAuthHeaders(HttpRequestMessage request, TmtrConnection connection)
	{
		request.Headers.Add("login", connection.Login);
		request.Headers.Add("password", connection.Password);
	}
}
