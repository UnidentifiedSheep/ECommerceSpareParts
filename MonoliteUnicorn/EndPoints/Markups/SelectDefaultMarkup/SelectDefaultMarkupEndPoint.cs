using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Markups.SelectDefaultMarkup;

public record SelectDefaultMarkupRequest(int MarkupGroupId); 
    
public class SelectDefaultMarkupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/markups/default", async (ISender sender, SelectDefaultMarkupRequest request, CancellationToken token) =>
        {
            var command = new SelectDefaultMarkupCommand(request.MarkupGroupId);
            await sender.Send(command, token);
            return Results.Ok();
        }).RequireAuthorization("AM")
        .WithGroup("Markups")
        .WithDescription("Установка дефолтной политики-наценки")
        .WithDisplayName("Установка дефолтной политики-наценки");
    }
}