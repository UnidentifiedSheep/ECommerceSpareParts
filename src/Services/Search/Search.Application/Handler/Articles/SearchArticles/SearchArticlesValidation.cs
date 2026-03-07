using Application.Common.Aot.Interfaces;
using Sannr;

namespace Search.Application.Handler.Articles.SearchArticles;

public class SearchArticlesValidation : IValidation<SearchArticlesQuery>
{
    public Task<ValidationResult> ValidateAsync(SearchArticlesQuery request)
    {
        var validationResults = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Query)) 
            validationResults.Add("Query", "Строка запроса не может быть пуста.");
        if (request.Limit is <= 0 or > 100) 
            validationResults.Add("Limit", "Количество ожидаемых артикулов должно быть больше 0 и не больше 100.");
        
        return Task.FromResult(validationResults);
    }
}