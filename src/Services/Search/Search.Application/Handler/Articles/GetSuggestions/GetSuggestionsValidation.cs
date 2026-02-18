using Application.Common.Aot.Interfaces;
using Sannr;

namespace Search.Application.Handler.Articles.GetSuggestions;

public class GetSuggestionsValidation : IValidation<GetSuggestionsQuery>
{
    public Task<ValidationResult> ValidateAsync(GetSuggestionsQuery request)
    {
        var validationResults = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Query)) validationResults.Add("Query", "Строка запроса не может быть пуста.");
        if (request.Limit <= 0) validationResults.Add("Limit", "Количество ожидаемых атодополнений должно быть больше 0");
        return Task.FromResult(validationResults);
    }
}