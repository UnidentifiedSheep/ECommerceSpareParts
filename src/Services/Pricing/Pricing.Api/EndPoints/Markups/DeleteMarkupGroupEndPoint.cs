using Api.Common.Extensions;
using Carter;
using Enums;
using MediatR;
using Pricing.Application.Handlers.Markups.DeleteMarkup;

namespace Pricing.Api.EndPoints.Markups;

public class DeleteMarkupGroupEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/markups/{groupId}", async (ISender sender, int groupId, CancellationToken cancellation) =>
            {
                var command = new DeleteMarkupGroupCommand(groupId);
                await sender.Send(command, cancellation);
                return Results.NoContent();
            }).WithTags("Markups")
            .WithDescription("Удаление группы наценок")
            .WithDisplayName("Удаление группы наценок")
            .RequireAnyPermission(PermissionCodes.MARKUP_DELETE);
    }
}