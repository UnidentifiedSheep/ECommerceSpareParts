using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Member.Vehicles;

namespace MonoliteUnicorn.EndPoints.Users.AddVehicleToGarage;

public record AddVehicleToGarageRequest(VehicleDto Vehicle);

public class AddVehicleToGarageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/vehicles/", async (ISender sender, AddVehicleToGarageRequest request, string userId, CancellationToken token) =>
        {
            var command = new AddVehicleToGarageCommand(request.Vehicle, userId);
            await sender.Send(command, token);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Users")
        .WithDescription("Добавление транспортного средства пользователю")
        .WithDisplayName("Добавление ТС AMW");
        
        app.MapPost("/users/me/vehicles/", async (ISender sender, AddVehicleToGarageRequest request, ClaimsPrincipal user, CancellationToken token) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new AddVehicleToGarageCommand(request.Vehicle, userId ?? "");
            await sender.Send(command, token);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithGroup("Users")
        .WithDescription("Добавление транспортного средства пользователю")
        .WithDisplayName("Добавление ТС Member");
    }
}