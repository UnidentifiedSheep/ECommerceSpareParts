using Main.Application.Handlers.Markups.DeleteMarkup;
using Carter;
using MediatR;

namespace Main.Api.EndPoints.Markups;

public class DeleteMarkupGroupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/markups/{groupId}", async (ISender sender, int groupId, CancellationToken cancellation) =>
            {
                var command = new DeleteMarkupGroupCommand(groupId);
                await sender.Send(command, cancellation);
                return Results.NoContent();
            }).RequireAuthorization("AM")
            .WithTags("Markups")
            .WithDescription("Удаление группы наценок")
            .WithDisplayName("Удаление группы наценок");
    }
}