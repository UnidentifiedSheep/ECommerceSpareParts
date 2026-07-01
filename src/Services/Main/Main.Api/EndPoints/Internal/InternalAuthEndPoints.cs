using System.Text.Json.Serialization;
using Main.Application.Handlers.Auth;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalServiceLoginRequest
{
    [JsonPropertyName("service")]
    public required string Service { get; init; }

    [JsonPropertyName("secret")]
    public required string ServiceSecret { get; init; }
}

public record InternalServiceLoginResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
}

public static class InternalAuthEndPoints
{
    public static RouteGroupBuilder AddInternalAuthEndPoints(this RouteGroupBuilder group)
    {
        var auth = group
            .MapGroup("/auth")
            .WithGroupName("Internal Authentication");

        auth.MapPost(
                "/token",
                async (
                    ISender sender,
                    InternalServiceLoginRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new InternalServiceLoginCommand(request.Service, request.ServiceSecret),
                        cancellationToken);

                    return Results.Ok(
                        new InternalServiceLoginResponse
                        {
                            Token = result.Token
                        });
                })
            .WithDisplayName("Internal service login")
            .WithName("InternalServiceLogin")
            .WithSummary("Выпустить токен внутреннего сервиса")
            .WithDescription("Авторизация внутреннего сервиса по секрету")
            .Accepts<InternalServiceLoginRequest>(false, "application/json")
            .Produces<InternalServiceLoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}