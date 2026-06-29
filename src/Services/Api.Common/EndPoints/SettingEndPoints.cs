using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Application.Common.Dtos;
using Application.Common.Handlers.Settings;
using Carter;
using Enums;
using MediatR;

namespace Api.Common.EndPoints;

public record GetSettingsResponse
{
    [JsonPropertyName("settings")]
    public required IReadOnlyList<SettingDto> Settings { get; init; }
}

public record UpdateSettingRequest
{
    [JsonPropertyName("json")]
    public required string Json { get; init; } = "{}";
}

public class SettingEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var settings = app.MapGroup("/settings")
            .WithTags("Settings");

        settings.MapGet("", async (
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetSettingsQuery(), ct);

                return Results.Ok(new GetSettingsResponse
                {
                    Settings = result.Settings
                });
            })
            .WithName("GetSettings")
            .WithDisplayName("Get settings")
            .Produces<GetSettingsResponse>()
            .RequireAllPermissions(PermissionCodes.OPTIONS_GET);

        settings.MapPut("{settingName}", async (
                ISender sender,
                string settingName,
                UpdateSettingRequest request,
                CancellationToken ct) =>
            {
                await sender.Send(
                    new UpdateSettingCommand(settingName, request.Json),
                    ct);

                return Results.NoContent();
            })
            .WithName("UpdateSetting")
            .WithDisplayName("Update setting")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAllPermissions(PermissionCodes.OPTIONS_GET);
    }
}
