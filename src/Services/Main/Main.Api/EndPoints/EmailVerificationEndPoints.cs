using Carter;
using Main.Application.Handlers.Auth.EmailVerification;
using MediatR;

namespace Main.Api.EndPoints;

public record VerifyEmailRequest(string Token);

public class EmailVerificationEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/users/emails/verification/verify",
                async (
                    ISender sender,
                    VerifyEmailRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new VerifyEmailCommand(request.Token),
                        cancellationToken);

                    return Results.NoContent();
                })
            .WithName("VerifyEmail")
            .WithTags("Email verifications")
            .WithSummary("Подтвердить почту")
            .WithDescription("Подтверждает почту одноразовым токеном из письма")
            .WithDisplayName("Подтверждение почты")
            .Accepts<VerifyEmailRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }
}
