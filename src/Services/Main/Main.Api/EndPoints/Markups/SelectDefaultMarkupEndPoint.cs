using Carter;
using Main.Application.Handlers.Markups.SelectDefaultMarkup;
using MediatR;

namespace Main.Api.EndPoints.Markups;

public record SelectDefaultMarkupRequest(int MarkupGroupId);

public class SelectDefaultMarkupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/markups/default",
                async (ISender sender, SelectDefaultMarkupRequest request, CancellationToken token) =>
                {
                    var command = new SelectDefaultMarkupCommand(request.MarkupGroupId);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Markups")
                .WithDescription("Установка дефолтной политики-наценки")
                .WithDisplayName("Установка дефолтной политики-наценки");
    }
}