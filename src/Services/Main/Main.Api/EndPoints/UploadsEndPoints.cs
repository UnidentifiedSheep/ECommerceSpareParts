using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Uploads;
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

public class UploadsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var uploads = app
            .MapGroup("/uploads")
            .WithTags("Uploads");

        uploads.MapPost("/create", async (
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
    }
}
