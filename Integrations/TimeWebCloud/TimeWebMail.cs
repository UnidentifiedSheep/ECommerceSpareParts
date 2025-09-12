using System.Collections.Specialized;
using System.Net.Http.Json;
using Core.Dtos.TimeWebCloud.Responses;
using Core.Interfaces;
using Integrations.Exceptions;
using Integrations.Models.TimeWebCloud;
using Mapster;
using Newtonsoft.Json;

namespace Integrations.TimeWebCloud;

public class TimeWebMail(HttpClient client) : ITimeWebMail
{
    public async Task<GetMailsResponse> GetMails(int limit = 100, int offset = 0, string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0) throw new WrongParamsException(nameof(limit), "Лимит должен быть больше 0");
        if (offset < 0) throw new WrongParamsException(nameof(offset), "Смещение не может быть меньше 0");
        var queryParams = new NameValueCollection
        {
            { "limit", limit.ToString() },
            { "offset", offset.ToString() }
        };
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add("search", search);
        var response = await client.GetAsync($"/api/v1/mail?{queryParams}", cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ??
                             throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }

        var result = JsonConvert.DeserializeObject<Responses.TimeWebCloud.GetMailsResponse>(responseString)!;
        return result.Adapt<GetMailsResponse>();
    }

    public async Task<GetMailsOfDomainResponse> GetMailsOfDomain(string domain, int limit = 100, int offset = 0,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0) throw new WrongParamsException(nameof(limit), "Лимит должен быть больше 0");
        if (offset < 0) throw new WrongParamsException(nameof(offset), "Смещение не может быть меньше 0");
        if (string.IsNullOrWhiteSpace(domain))
            throw new WrongParamsException(nameof(domain), "Имя домена не может быть пустым");
        var queryParams = new NameValueCollection
        {
            { "limit", limit.ToString() },
            { "offset", offset.ToString() }
        };
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add("search", search);
        var response = await client.GetAsync($"/api/v1/mail/domains/{domain}?{queryParams}", cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ??
                             throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }

        var result = JsonConvert.DeserializeObject<Responses.TimeWebCloud.GetMailsOfDomainResponse>(responseString)!;
        return result.Adapt<GetMailsOfDomainResponse>();
    }

    public async Task CreateMail(string domain, string mailBox, string password, string comment = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mailBox))
            throw new WrongParamsException(nameof(mailBox), "Адрес не может быть пустым");
        if (string.IsNullOrWhiteSpace(password))
            throw new WrongParamsException(nameof(password), "Пароль не может быть пустым");
        if (string.IsNullOrWhiteSpace(domain))
            throw new WrongParamsException(nameof(domain), "Имя домена не может быть пустым");
        var content = JsonContent.Create(new
        {
            mailbox = mailBox,
            password,
            comment
        });
        var response = await client.PostAsync($"/api/v1/mail/domains/{domain}", content, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorModel = JsonConvert.DeserializeObject<ExceptionModel>(responseString) ??
                             throw new UnableDeserializeErrorException(responseString);
            throw new NotSuccessfulRequestException(errorModel);
        }
    }
}