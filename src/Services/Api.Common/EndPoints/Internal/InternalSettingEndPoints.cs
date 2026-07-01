using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Application.Common.Handlers.Settings;
using Enums;
using MediatR;

namespace Api.Common.EndPoints.Internal;

public record InternalGetRawSettingResponse
{
    [JsonPropertyName("json")]
    public required string Json { get; init; }
}

public static class InternalSettingEndPoints
{
    public static RouteGroupBuilder AddInternalSettingEndPoints(this RouteGroupBuilder group)
    {
        var settings = group
            .MapGroup("/settings")
            .WithGroupName("Internal Settings")
            .WithTags("InternalSettings");

        settings.MapGet("{systemName}", async (
                ISender sender,
                string systemName,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetRawSettingQuery(systemName),
                    cancellationToken);

                return Results.Ok(new InternalGetRawSettingResponse
                {
                    Json = result.Value
                });
            })
            .WithName("InternalGetRawSetting")
            .WithDisplayName("Internal service raw setting")
            .WithSummary("Получить raw настройку для внутреннего сервиса")
            .WithDescription("Получение JSON настройки по системному имени для внутренних интеграций")
            .Produces<InternalGetRawSettingResponse>()
            .RequireAnyRole(Role.System)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
