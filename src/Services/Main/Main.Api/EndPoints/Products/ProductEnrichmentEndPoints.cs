using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Product.Enrichment;
using Main.Application.Handlers.ProductEnrichment.GetCatalogueCandidatesForReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Products;

public record GetCatalogueCandidatesForReviewRequest : PaginationQueryModel
{
	[FromQuery(Name = "productId")]
	public int? ProductId { get; init; }

	[FromQuery(Name = "sku")]
	public string? Sku { get; init; }
}

public record GetCatalogueCandidatesForReviewResponse
{
	[JsonPropertyName("candidates")]
	public required IReadOnlyList<CatalogueCandidateReviewDto> Candidates { get; init; }
}

public static class ProductEnrichmentEndPoints
{
	public static RouteGroupBuilder MapProductEnrichmentEndPoints(this RouteGroupBuilder products)
	{
		products
			.MapGet(
				"/enrichment",
				async (
					ISender sender, [AsParameters] GetCatalogueCandidatesForReviewRequest request,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new GetCatalogueCandidatesForReviewQuery(
							request.ProductId,
							request.Sku,
							request),
						cancellationToken);

					return Results.Ok(
						new GetCatalogueCandidatesForReviewResponse
						{
							Candidates = result.Candidates
						});
				})
			.WithName("GetCatalogueCandidatesForReview")
			.WithSummary("Получить кандидатов каталога для проверки")
			.WithDescription(
				"Получение кандидатов каталога вместе с сопоставленными товарами и исходными товарами поставщиков")
			.Produces<GetCatalogueCandidatesForReviewResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.RequireAnyPermission(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW);

		return products;
	}
}
