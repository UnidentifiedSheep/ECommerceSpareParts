using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.RegisterAccount
{
    public record RegisterRequest(string Email, string UserName, string Password, string Name, string Surname);
    public class RegisterEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register/", async (RegisterRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterCommand>();
                await sender.Send(command);
                return Results.Ok();
            }).WithName("RegisterUser")
            .WithGroup("Auth")
            .Produces<Unit>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register User")
            .WithDescription("Register User");
        }
    }
}
