using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Uploads;
using Main.Application.Handlers.Uploads;
using Main.Application.Handlers.Uploads.GetUploads;
using MediatR;

namespace Main.Api.EndPoints;

public record CreateUploadRequest
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("contentType")]
    public required string ContentType { get; init; }
}

public record CreateUploadResponse(string UploadUrl);

public record GetUploadsResponse
{
    [JsonPropertyName("files")]
    public required IReadOnlyList<FileDto> Files { get; init; }

    [JsonPropertyName("nextCursor")]
    public string? NextContinuationToken { get; init; }

    [JsonPropertyName("hasMore")]
    public required bool HasMore { get; init; }
}

public record CompleteUploadRequest
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }
}

public class UploadsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var uploads = app
            .MapGroup("/uploads")
            .WithTags("Uploads");

        uploads.MapGet(
                "",
                async (
                    ISender sender,
                    string? cursor,
                    int size,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new GetUploadsQuery(new Cursor<string?>(cursor, size)),
                        token);

                    return Results.Ok(
                        new GetUploadsResponse
                        {
                            Files = result.Files,
                            NextContinuationToken = result.NextContinuationToken,
                            HasMore = result.HasMore
                        });
                })
            .WithName("GetUploads")
            .WithSummary("Get uploads")
            .WithDisplayName("Get Uploads")
            .Produces<GetUploadsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.UPLOADS_CREATE);

        uploads.MapPost(
                "/create",
                async (
                    ISender sender,
                    CreateUploadRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new CreateUploadRequestCommand(request.FileName, request.ContentType),
                        token);

                    return Results.Ok(new CreateUploadResponse(result.UploadUrl));
                })
            .WithName("CreateUploadRequest")
            .WithSummary("Create upload request")
            .WithDisplayName("Create Upload Request")
            .Accepts<CreateUploadRequest>(false, "application/json")
            .Produces<CreateUploadResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.UPLOADS_CREATE);

        uploads.MapPost(
                "/complete",
                async (
                    ISender sender,
                    CompleteUploadRequest request,
                    CancellationToken token) =>
                {
                    await sender.Send(new CompleteUploadCommand(request.FileName), token);
                    return Results.Ok();
                })
            .WithName("CompleteUpload")
            .WithSummary("Complete upload")
            .WithDisplayName("Complete Upload")
            .Accepts<CompleteUploadRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.UPLOADS_CREATE);
    }
}