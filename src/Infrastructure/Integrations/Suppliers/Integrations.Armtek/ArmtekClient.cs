using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Abstractions.Models.Options;
using Enums;
using Integrations.Armtek.Interfaces;
using Integrations.Armtek.Models;
using Integrations.Armtek.Requests;
using Integrations.Armtek.Responses;
using Integrations.Client.Core;
using Integrations.Common;
using Integrations.Supplier.Connections;

namespace Integrations.Armtek;

public class ArmtekClient(
	HttpClient client,
	IConnectionProvider<ArmtekConnection> connectionProvider,
	ProjectJsonOptions jsonOptions) : ClientBase(jsonOptions), IArmtekClient
{
	private const string FormatField = "format";

	private const string JsonFormat = "json";

	private const string SearchPath = $"/api/ws_search/search?{FormatField}={JsonFormat}";

	private const string AssortmentSearchPath = $"/api/ws_search/assortment_search?{FormatField}={JsonFormat}";

	public async Task<Response<IReadOnlyList<SearchItem>>> SearchAsync(
		SearchRequest request,
		CancellationToken token = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.StorageCode);

		var connection = await connectionProvider.GetConnectionAsync(token);
		if (connection.FindStorage(request.StorageCode) is not { } storage)
			return Response<IReadOnlyList<SearchItem>>.Fail(
				HttpStatusCode.UnprocessableEntity,
				$"Armtek storage binding not found for '{request.StorageCode}'");

		var form = new Dictionary<string, string>
		{
			["VKORG"] = connection.SalesOrganization,
			["KUNNR_RG"] = connection.CustomerCode,
			["PIN"] = request.Number,
			["QUERY_TYPE"] = request.WithAnalogues ? "2" : "1",
			["KUNNR_ZA"] = storage.PointCode,
			["INCOTERMS"] = storage.DeliveryMode == DeliveryMode.Pickup ? "1" : "0"
		};

		if (!string.IsNullOrWhiteSpace(request.Brand))
			form["BRAND"] = request.Brand;
		if (!string.IsNullOrWhiteSpace(connection.ContractCode))
			form["VBELN"] = connection.ContractCode;

		return await SendAsync<SearchResponse, SearchItem>(
			connection,
			SearchPath,
			form,
			x => x.Items,
			token);
	}

	public async Task<Response<IReadOnlyList<AssortmentItem>>> AssortmentSearchAsync(
		AssortmentSearchRequest request,
		CancellationToken token = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);

		var connection = await connectionProvider.GetConnectionAsync(token);
		var form = new Dictionary<string, string>
		{
			["VKORG"] = connection.SalesOrganization,
			["PIN"] = request.Number
		};

		return await SendAsync<AssortmentSearchResponse, AssortmentItem>(
			connection,
			AssortmentSearchPath,
			form,
			x => x.Items,
			token);
	}

	private async Task<Response<IReadOnlyList<TItem>>> SendAsync<TResponse, TItem>(
		ArmtekConnection connection,
		string path,
		Dictionary<string, string> form,
		Func<TResponse, IReadOnlyList<TItem>> selector,
		CancellationToken token)
		where TResponse : ArmtekResponse
	{
		form[FormatField] = JsonFormat;

		using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(connection.BaseUrl), path));
		httpRequest.Headers.Authorization = CreateAuthorization(connection);
		httpRequest.Content = new FormUrlEncodedContent(form);
		httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
		{
			CharSet = Encoding.UTF8.WebName
		};

		using var response = await client.SendAsync(httpRequest, token);
		var result = await ReadResponse<TResponse>(response, token);

		if (!result.Success)
			return Response<IReadOnlyList<TItem>>.FromFail(result);

		if (result.Value is not { } body)
			return Response<IReadOnlyList<TItem>>.Fail(response.StatusCode, "Response body is null");

		return body.IsSuccess
			? Response<IReadOnlyList<TItem>>.Ok(selector(body))
			: Response<IReadOnlyList<TItem>>.Fail(GetStatusCode(body), GetError(body));
	}

	private static AuthenticationHeaderValue CreateAuthorization(ArmtekConnection connection)
		=> new(
			"Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connection.Login}:{connection.Password}")));

	private static HttpStatusCode GetStatusCode(ArmtekResponse body)
		=> body.Status is >= 400 and < 600 ? (HttpStatusCode)body.Status : HttpStatusCode.BadGateway;

	private static string GetError(ArmtekResponse body)
		=> body.Errors.Count == 0
			? $"Armtek returned status {body.Status}"
			: string.Join("; ", body.Errors.Select(x => x.Text));
}
