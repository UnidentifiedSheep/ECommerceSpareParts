using System.Web;
using Core.Services.Armtek.Models;
using Integrations.Models.Armtek;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Integrations.Armtek;


public class UserSettingsService
{
    private readonly string _baseUrl; 
    private readonly HttpClient _client;

    /// <summary>
    /// Auth must be included to client factory headers.
    /// </summary>
    /// <param name="clientFactory">The name of client config must be "ArmtekClient".</param>
    /// <param name="config">Parameters should be in "Armtek" field.</param>
    public UserSettingsService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        var httpClientFactory = clientFactory;
        _baseUrl = config["Armtek:BaseUrl"] ?? throw new NullReferenceException("BaseUrl not found in Armtek:BaseUrl");
        _client = httpClientFactory.CreateClient("ArmtekClient");
    }
    
    /// <summary>
    /// Сервис получения сбытовых организаций клиента "getUserVkorgList"
    /// </summary>
    /// <returns>Список vkorg</returns>
    /// <exception cref="HttpRequestException">Если ответ вернулся с каким либо не успешным кодом.</exception>
    public async Task<List<VkorgModel>> GetVkorgListAsync(CancellationToken cancellationToken = default)
    {
        var uri = new UriBuilder(_baseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/ws_user/getUserVkorgList";
        uri.Query = "?format=json";
        var response = await _client.GetAsync(uri.Uri, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Unable to get user's Vkorg list: {responseString}");
        return JsonConvert.DeserializeObject<List<VkorgModel>>(responseString)!;
    }

    /// <summary>
    /// Сервис получения структуры клиента
    /// </summary>
    /// <param name="vkorg">Сбытовая организация</param>
    /// <param name="structure">Получить структуру клиента</param>
    /// <param name="getAsFtpData">Получить данные FTP клиента</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">vkorg - либо пустая строка, либо длиннее 4 символов</exception>
    /// <exception cref="HttpRequestException">Запрос завершился безуспешно</exception>
    public async Task<List<UserInfoModel>> GetUserInfoAsync(string vkorg, bool structure = true, bool getAsFtpData = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vkorg) || vkorg.Length > 4) throw new ArgumentException("Vkorg value is invalid.");
        var uri = new UriBuilder(_baseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/ws_user/getUserInfo";
        var query = HttpUtility.ParseQueryString("format=json");
        query["VKORG"] = vkorg;
        query["STRUCTURE"] = Convert.ToInt32(structure).ToString();
        uri.Query = query.ToString();
        var response = await _client.PostAsync(uri.Uri, null, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Unable to get user info: {responseString}");
        return JsonConvert.DeserializeObject<List<UserInfoModel>>(responseString)!;
    }

    /// <summary>
    /// Сервис получения списка брендов
    /// </summary>
    /// <param name="brandName">Наименование бренда для поиска</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Список брендов</returns>
    /// <exception cref="HttpRequestException">Запрос не завершился успешно.</exception>
    /// <exception cref="ArgumentException">Длина brandName больше 100 символов.</exception>
    public async Task<List<string>> GetBrandListAsync(string? brandName = null, CancellationToken cancellationToken = default)
    {
        string name = brandName?.Trim() ?? "";
        if (name.Length > 100) throw new ArgumentException("Brand length must be shorter then 100 characters.");
        var uri = new UriBuilder(_baseUrl);
        uri.Path = uri.Path.TrimEnd('/') + "/ws_user/getBrandList?format=json";
        if (string.IsNullOrWhiteSpace(name)) uri.Path += $"&BRAND={brandName}";
        var response = await _client.GetAsync(uri.Uri, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Unable to get brands: {responseString}");
        return JsonConvert.DeserializeObject<List<string>>(responseString)!;
    }
}