using System.Text.Json.Serialization;
using Abstractions.Models.Validation;
using Api.Common.Response;
using Contracts.Articles;
using Search.Abstractions.Dtos;
using Search.Api.EndPoints;
using Search.Entities;
using ContractArticle = Contracts.Models.Articles.Article;

namespace Search.Api.Contexts;

[JsonSerializable(typeof(Article[]))]
[JsonSerializable(typeof(ArticleDto[]))]
[JsonSerializable(typeof(ErrorResponse[]))]
[JsonSerializable(typeof(ValidationErrorModel[]))]
[JsonSerializable(typeof(AddArticleRequest))]
[JsonSerializable(typeof(GetArticleResponse))]
[JsonSerializable(typeof(GetSuggestionsResponse))]
[JsonSerializable(typeof(SearchArticlesResponse))]
[JsonSerializable(typeof(ArticlesCreatedEvent))]
[JsonSerializable(typeof(ContractArticle), TypeInfoPropertyName = "ContractArticleType")]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}