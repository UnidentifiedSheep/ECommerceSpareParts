using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Markups.DeleteMarkupGroup;

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
        .WithGroup("Markups")
        .WithDescription("Удаление группы наценок")
        .WithDisplayName("Удаление группы наценок");
    }
}