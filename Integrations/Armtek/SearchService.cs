using System.Web;
using Integrations.Models.Armtek;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Integrations.Armtek;

public class SearchService
{
    private readonly string _baseUrl;
    private readonly HttpClient _client;

    /// <summary>
    ///     Auth must be included to client factory headers.
    /// </summary>
    /// <param name="clientFactory">The name of client config must be "ArmtekClient".</param>
    /// <param name="config">Parameters should be in "Armtek" field.</param>
    public SearchService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        var httpClientFactory = clientFactory;
        _baseUrl = config["Armtek:BaseUrl"] ?? throw new NullReferenceException("BaseUrl not found in Armtek:BaseUrl");
        _client = httpClientFactory.CreateClient("ArmtekClient");
    }

    /// <summary>
    ///     Cервис поиска по ассортименту
    /// </summary>
    /// <param name="vkorg">Сбытовая организация</param>
    /// <param name="articleNumber">Номер артикула</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Список результатов поиска</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<List<ArticleModel>> AssortementSearch(string vkorg, string articleNumber,
        CancellationToken cancellationToken = default)
    {
        if (vkorg.Length > 4 || articleNumber.Length > 39)
            throw new ArgumentException("vkorg length must be less than 4 and articleNumber must be shorter then 40");
        var uri = new UriBuilder(_baseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/ws_search/assortment_search";
        var query = HttpUtility.ParseQueryString("");
        query["VKORG"] = vkorg;
        query["PIN"] = articleNumber;
        uri.Query = query.ToString();
        var response = await _client.GetAsync(uri.Uri, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Unable to get assortment: {responseString}");
        return JsonConvert.DeserializeObject<List<ArticleModel>>(responseString)!;
    }

    public async Task<int> Search()
    {
        throw new NotImplementedException();
    }
}