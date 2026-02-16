using System.Text.Json.Serialization;
using Abstractions.Models.Validation;
using Api.Common.Response;
using Search.Abstractions.Dtos;
using Search.Api.EndPoints;
using Search.Entities;

namespace Search.Api.Contexts;

[JsonSerializable(typeof(Article[]))]
[JsonSerializable(typeof(ArticleDto[]))]
[JsonSerializable(typeof(AddArticleRequest[]))]
[JsonSerializable(typeof(GetArticleResponse[]))]
[JsonSerializable(typeof(ErrorResponse[]))]
[JsonSerializable(typeof(ValidationErrorModel[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}