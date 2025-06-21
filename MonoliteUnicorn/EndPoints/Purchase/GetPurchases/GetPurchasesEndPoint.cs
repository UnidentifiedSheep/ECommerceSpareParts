using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using AmwPurchaseDto = MonoliteUnicorn.Dtos.Amw.Purchase.PurchaseDto;
using MemberPurchaseDto = MonoliteUnicorn.Dtos.Member.Purchase.PurchaseDto;

namespace MonoliteUnicorn.EndPoints.Purchase.GetPurchases;

public class GetPurchasesRequest
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; set; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "viewCount")] public int ViewCount { get; set; }
    [FromQuery(Name = "supplierId")] public string? SupplierId { get; set; }
    [FromQuery(Name = "currencyId")] public int? CurrencyId { get; set; }
    [FromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; set; }
}

public record GetPurchasesAmwResponse(IEnumerable<AmwPurchaseDto> Purchases);
public record GetPurchasesMemberResponse(IEnumerable<MemberPurchaseDto> Purchases);

public class GetPurchasesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/purchases/", async (ISender sender, [AsParameters] GetPurchasesRequest request, ClaimsPrincipal user, CancellationToken token) =>
        {
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (roles.IsAnyMatchInvariant("admin", "moderator", "worker")) 
                return await GetAmw(sender, request, token);
            if (roles.IsAnyMatchInvariant("member"))
                return await GetMember(sender, request, userId, token);
            return Results.Unauthorized();
        }).RequireAuthorization()
        .WithGroup("Purchases")
        .WithDescription("Получение списка покупок")
        .WithDisplayName("Получение покупок");
    }

    private async Task<IResult> GetAmw(ISender sender, GetPurchasesRequest request, CancellationToken token)
    {
        var query = request.Adapt<GetPurchasesAmwQuery>();
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetPurchasesAmwResponse>();
        return Results.Ok(response);
    }
    
    private async Task<IResult> GetMember(ISender sender, GetPurchasesRequest request, string userId, CancellationToken token)
    {
        var query = new GetPurchaseMemberQuery(request.RangeStartDate, request.RangeEndDate, request.Page, request.ViewCount, userId, request.CurrencyId, request.SortBy, request.SearchTerm);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetPurchasesMemberResponse>();
        return Results.Ok(response);
    }
}