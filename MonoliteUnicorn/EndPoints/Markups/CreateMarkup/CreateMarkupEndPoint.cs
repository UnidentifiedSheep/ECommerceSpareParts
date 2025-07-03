using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Markups;

namespace MonoliteUnicorn.EndPoints.Markups.CreateMarkup;

public record CreateMarkupRequest(IEnumerable<NewMarkupRangeDto> Ranges, int CurrencyId, string? GroupName); 
public record CreateMarkupResponse(int GroupId);
    
public class CreateMarkupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/markups", async (ISender sender, CreateMarkupRequest request, CancellationToken cancellationToken) =>
        {
            var command = request.Adapt<CreateMarkupCommand>();
            var result = await sender.Send(command, cancellationToken);
            var response = new CreateMarkupResponse(result.GroupId);
            return Results.Created($"/markups/{result.GroupId}", response);
        }).RequireAuthorization("AM")
        .WithGroup("Markups")
        .WithDescription("Создание группы наценок")
        .WithDisplayName("Создание группы наценок");
    }
}