using Api.Common.Extensions;
using Carter;
using Core.Models;
using Main.Application.Handlers.Options.GetEmailOptions;
using MediatR;

namespace Main.Api.EndPoints.Options;

public record GetEmailOptionsResponse(UserEmailOptions EmailOptions);

public class GetEmailOptionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/options/emails", async (ISender sender, CancellationToken token) =>
        {
            var result = await sender.Send(new GetEmailOptionsQuery(), token);
            return Results.Ok(new GetEmailOptionsResponse(result.EmailOptions));
        }).WithTags("Options")
        .WithDescription("Получение параметров почт")
        .WithDisplayName("Получение параметров почт")
        .RequireAnyPermission("USERS.CREATE");
    }
}