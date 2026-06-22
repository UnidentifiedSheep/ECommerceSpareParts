using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Balance;
using Main.Application.Handlers.Users.GetUserDiscount;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserFinancialInfoResponse
{
    [JsonPropertyName("financialProfile")]
    public required UserFinancialProfileDto? FinancialProfile { get; init; }
    
    [JsonPropertyName("baseCurrency")]
    public required CurrencyDto BaseCurrency { get; init; }
    
    [JsonPropertyName("balances")]
    public required IEnumerable<UserBalanceDto> Balances { get; init; }
}

public static class UserFinancialEndPoints
{
    public static RouteGroupBuilder MapUserFinancialEndPoints(this RouteGroupBuilder users)
    {
        users.MapGet("/{id:guid}/finances", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetUserFinancialInfoQuery(id), token);
                return Results.Ok(new GetUserFinancialInfoResponse
                {
                    Balances = result.Balances,
                    BaseCurrency = result.BaseCurrency,
                    FinancialProfile = result.FinancialProfile
                });
            })
            .WithName("GetUserFinancialInfo")
            .WithSummary("Получить информацию по финанцам пользователя")
            .WithDescription("Получение финансовой информации пользователя")
            .WithDisplayName("Получение финансовой информации пользователя")
            .Produces<GetUserFinancialInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);
        
        return users;
    }
}