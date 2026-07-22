using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;
using Main.Application.Handlers.Balance.UpdateOrganizationFinancialProfile;
using MediatR;

namespace Main.Api.EndPoints.Organizations;

public record GetOrganizationFinancialInfoResponse
{
    [JsonPropertyName("financialProfile")]
    public required OrganizationFinancialProfileDto? FinancialProfile { get; init; }

    [JsonPropertyName("baseCurrency")]
    public required CurrencyDto BaseCurrency { get; init; }

    [JsonPropertyName("balances")]
    public required IEnumerable<OrganizationBalanceDto> Balances { get; init; }
}

public record UpdateOrganizationFinancialInfoRequest
{
    [JsonPropertyName("financialProfile")]
    public required PatchOrganizationFinancialProfileDto FinancialProfile { get; init; }
}

public record UpdateOrganizationFinancialInfoResponse
{
    [JsonPropertyName("financialProfile")]
    public required OrganizationFinancialProfileDto FinancialProfile { get; init; }
}

public class OrganizationFinancialEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var organizations = app.MapGroup("/organizations")
            .WithTags("Organizations");

        organizations.MapGet(
                "/{organizationId:guid}/finances",
                async (
                    ISender sender,
                    Guid organizationId,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetOrganizationFinancialInfoQuery(organizationId),
                        cancellationToken);

                    return Results.Ok(
                        new GetOrganizationFinancialInfoResponse
                        {
                            Balances = result.Balances,
                            BaseCurrency = result.BaseCurrency,
                            FinancialProfile = result.FinancialProfile
                        });
                })
            .WithName("GetOrganizationFinancialInfo")
            .WithSummary("Получить финансовую информацию организации")
            .WithDescription("Получение финансового профиля и балансов организации")
            .Produces<GetOrganizationFinancialInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.BALANCES_FINANCES_GET);

        organizations.MapPatch(
                "/{organizationId:guid}/finances",
                async (
                    ISender sender,
                    UpdateOrganizationFinancialInfoRequest request,
                    Guid organizationId,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new UpdateOrganizationFinancialProfileCommand(
                            organizationId,
                            request.FinancialProfile),
                        cancellationToken);

                    return Results.Ok(
                        new UpdateOrganizationFinancialInfoResponse
                        {
                            FinancialProfile = result.Profile
                        });
                })
            .WithName("UpdateOrganizationFinancialInfo")
            .WithSummary("Обновить финансовый профиль организации")
            .WithDescription("Обновление финансового профиля организации")
            .Produces<UpdateOrganizationFinancialInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.BALANCES_FINANCES_UPDATE);
    }
}
