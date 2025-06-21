using System.Text;

namespace Core.HttpCommunication
{
	public class HttpAsync(HttpClient httpClient)
	{
		public async Task<string?> GetAsync(string service, string uri, CancellationToken cancellationToken = default, params string[] query)
		{
			var endUri = GetEndUri(service, uri);
			var uriBuilder = new UriBuilder(endUri);
			
            uriBuilder.Query = GetAsQuery(query);

			var response = await httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
			return await response.Content.ReadAsStringAsync(cancellationToken);
		}
		public async Task<string?> PostAsync(string service, string uri, string? content = null, CancellationToken cancellationToken = default)
		{
			var endUri = GetEndUri(service, uri);
			HttpContent? con = content == null ? null : new StringContent(content);
			var response = await httpClient.PostAsync(endUri, con, cancellationToken);
			return await response.Content.ReadAsStringAsync(cancellationToken);
		}
		public async Task<string?> PatchAsync(string service, string uri, string? content = null, CancellationToken cancellationToken = default)
		{
			var endUri = GetEndUri(service, uri);
			HttpContent? con = content == null ? null : new StringContent(content);
			var response = await httpClient.PatchAsync(endUri, con, cancellationToken);
			return await response.Content.ReadAsStringAsync(cancellationToken);
		}
		public async Task<string?> DeleteAsync(string service, string uri, CancellationToken cancellationToken = default, params string[] query)
		{
			var endUri = GetEndUri(service, uri);
			var uriBuilder = new UriBuilder(endUri);
			uriBuilder.Query = GetAsQuery(query);
			var response = await httpClient.DeleteAsync(uriBuilder.Uri, cancellationToken);
			return await response.Content.ReadAsStringAsync(cancellationToken);
		}
		private string GetEndUri(string service, string uri)
		{
			var baseUri = ServicesList.TryGetService(service).TrimEnd('/').TrimEnd('\\');
			return baseUri + "/" + uri.TrimStart('/').TrimStart('\\').TrimEnd('/').TrimEnd('\\');
		}
		private string GetAsQuery(params string[] query)
		{
			var endQuery = new StringBuilder();
			foreach (var item in query)
				endQuery.Append(item + "&");
			var result = endQuery.ToString().TrimEnd('&');
			endQuery.Clear();	
			return result;
		}
	}
}
