using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductCharacteristics.DeleteCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.UpsertCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record UpsertCharacteristicsRequest(IReadOnlyCollection<UpsertCharacteristicsDto> Characteristics);

public record EditCharacteristicsRequest(string Value);

public record GetProductCharacteristicsResponse(IReadOnlyList<ProductCharacteristicDto> Characteristics);

public static class ProductCharacteristicsEndPoints
{
	public static RouteGroupBuilder MapProductCharacteristicsEndPoints(this RouteGroupBuilder products)
	{
		products
			.MapPut(
				"/characteristics/",
				async (
					ISender sender, UpsertCharacteristicsRequest request,
					CancellationToken token) =>
				{
					await sender.Send(new UpsertCharacteristicsCommand(request.Characteristics), token);
					return Results.Ok();
				})
			.WithName("UpsertProductCharacteristics")
			.WithDescription("Создание или обновление характеристик артикула")
			.Accepts<UpsertCharacteristicsRequest>(false, "application/json")
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Создать или обновить характеристики продуктов")
			.RequireAllPermissions(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE, PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE);

		products
			.MapDelete(
				"/{productId:int}/characteristics/{characteristicId:int}",
				async (
					ISender sender, int productId,
					int characteristicId, CancellationToken token) =>
				{
					await sender.Send(new DeleteCharacteristicsCommand(productId, characteristicId), token);
					return Results.Ok();
				})
			.WithName("DeleteProductCharacteristic")
			.WithDescription("Удаление характеристики")
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Удалить характеристику продукта")
			.RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE);

		products
			.MapPut(
				"/{productId:int}/characteristics/{characteristicId:int}",
				async (
					ISender sender, int productId,
					int characteristicId, EditCharacteristicsRequest request,
					CancellationToken token) =>
				{
					await sender.Send(
						new UpsertCharacteristicsCommand(
						[
							new UpsertCharacteristicsDto
							{
								ProductId = productId,
								CharacteristicId = characteristicId,
								Value = request.Value
							}
						]),
						token);
					return Results.Ok();
				})
			.WithName("UpsertProductCharacteristic")
			.WithDescription("Создание или обновление характеристики")
			.Accepts<EditCharacteristicsRequest>(false, "application/json")
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Создать или обновить характеристику продукта")
			.RequireAllPermissions(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE, PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE);

		products
			.MapGet(
				"/{productId:int}/characteristics",
				async (
					ISender sender, int productId,
					[AsParameters] PaginationQueryModel queryParams, CancellationToken token) =>
				{
					var result = await sender.Send(
						new GetCharacteristicsQuery(productId, queryParams),
						token);
					return Results.Ok(new GetProductCharacteristicsResponse(result.Characteristics));
				})
			.WithName("GetProductCharacteristics")
			.WithDescription("Получить характеристики артикула")
			.Produces<GetProductCharacteristicsResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Получить характеристики продукта");

		return products;
	}
}
