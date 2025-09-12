using System.Net.Http.Json;
using Core.Interfaces;
using Integrations.Exceptions;
using Integrations.Interfaces;
using Integrations.Models.TimeWebCloud;
using Integrations.Responses.TimeWebCloud;
using Mapster;
using Newtonsoft.Json;

namespace Integrations.TimeWebCloud;

public class TimeWebMail(HttpClient client) : ITimeWebMail
{
    public async Task<Core.Dtos.TimeWebCloud.Responses.GetMailsResponse> GetMails(int limit = 100, int offset = 0, string? search = null, CancellationToken cancellationToken = default)
    {
        if (limit <= 0) throw new WrongParamsException(nameof(limit), "Лимит должен быть больше 0");
        if (offset < 0) throw new WrongParamsException(nameof(offset), "Смещение не может быть меньше 0");
        var queryParams = new System.Collections.Specialized.NameValueCollection
        {
            { "limit", limit.ToString() },
            { "offset", offset.ToString() }
        };
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add("search", search);
        var response = await client.GetAsync($"/api/v1/mail?{queryParams}", cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ?? throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }
        var result = JsonConvert.DeserializeObject<GetMailsResponse>(responseString)!;
        return result.Adapt<Core.Dtos.TimeWebCloud.Responses.GetMailsResponse>();
    }

    public async Task<Core.Dtos.TimeWebCloud.Responses.GetMailsOfDomainResponse> GetMailsOfDomain(string domain, int limit = 100, int offset = 0, string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0) throw new WrongParamsException(nameof(limit), "Лимит должен быть больше 0");
        if (offset < 0) throw new WrongParamsException(nameof(offset), "Смещение не может быть меньше 0");
        if (string.IsNullOrWhiteSpace(domain)) throw new WrongParamsException(nameof(domain), "Имя домена не может быть пустым");
        var queryParams = new System.Collections.Specialized.NameValueCollection
        {
            { "limit", limit.ToString() },
            { "offset", offset.ToString() }
        };
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add("search", search);
        var response = await client.GetAsync($"/api/v1/mail/domains/{domain}?{queryParams}", cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ?? throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }
        var result = JsonConvert.DeserializeObject<GetMailsOfDomainResponse>(responseString)!;
        return result.Adapt<Core.Dtos.TimeWebCloud.Responses.GetMailsOfDomainResponse>();
    }
    
    public async Task CreateMail(string domain ,string mailBox, string password, string comment = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mailBox)) throw new WrongParamsException(nameof(mailBox), "Адрес не может быть пустым");
        if (string.IsNullOrWhiteSpace(password)) throw new WrongParamsException(nameof(password), "Пароль не может быть пустым");
        if (string.IsNullOrWhiteSpace(domain)) throw new WrongParamsException(nameof(domain), "Имя домена не может быть пустым");
        var content = JsonContent.Create(new
        {
            mailbox = mailBox,
            password = password,
            comment = comment
        });
        var response = await client.PostAsync($"/api/v1/mail/domains/{domain}", content,cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ?? throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }
    }
}