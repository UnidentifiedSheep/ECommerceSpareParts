using System.Text.Json.Serialization;
using Carter;
using Main.Application.Handlers.Auth.InternalServiceLogin;
using MediatR;

namespace Main.Api.EndPoints.Internal.Auth;

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

public class InternalServiceLoginEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/auth/token", async (
                ISender sender,
                InternalServiceLoginRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new InternalServiceLoginCommand(request.Service, request.ServiceSecret),
                    cancellationToken);

                return Results.Ok(new InternalServiceLoginResponse
                {
                    Token = result.Token
                });
            }).WithGroupName("Internal Authentication")
            .WithDisplayName("Internal service login")
            .WithName("InternalServiceLogin");
    }
}