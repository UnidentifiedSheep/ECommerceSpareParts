using Main.Application.Handlers.Markups.CreateMarkup;
using Carter;
using Core.Dtos.Amw.Markups;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Markups;

public record CreateMarkupRequest(
    IEnumerable<NewMarkupRangeDto> Ranges,
    int CurrencyId,
    string? GroupName,
    decimal MarkupForUnknownRange);

public record CreateMarkupResponse(int GroupId);

public class CreateMarkupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/markups",
                async (ISender sender, CreateMarkupRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.Adapt<CreateMarkupCommand>();
                    var result = await sender.Send(command, cancellationToken);
                    var response = new CreateMarkupResponse(result.GroupId);
                    return Results.Created($"/markups/{result.GroupId}", response);
                }).RequireAuthorization("AM")
            .WithTags("Markups")
            .WithDescription("Создание группы наценок")
            .WithDisplayName("Создание группы наценок");
    }
}