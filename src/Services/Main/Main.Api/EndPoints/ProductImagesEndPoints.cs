using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Modules.Product.Application.Commands.Products.MapImgsToProduct;
using MediatR;

namespace Main.Api.EndPoints;

public class ProductImagesEndPoints : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
		=> app
			.MapGroup("/products")
			.WithTags("Products")
			.MapPost(
				"/{productId:int}/imgs/",
				async (
					ISender sender, int productId,
					IFormFileCollection files, CancellationToken token) =>
				{
					var images = files
						.Select(file => new ProductImageUpload(
							Path.GetExtension(file.FileName),
							file.OpenReadStream))
						.ToArray();
					await sender.Send(new MapImgsToProductCommand(productId, images), token);
					return Results.Ok();
				})
			.DisableAntiforgery()
			.WithMetadata()
			.WithName("AddProductImages")
			.WithSummary("Добавить изображения продукта")
			.WithDescription("Добавление изображений продукта")
			.Accepts<IFormFileCollection>("multipart/form-data")
			.Produces(200)
			.ProducesProblem(400)
			.ProducesProblem(404)
			.RequireAnyPermission(PermissionCodes.ARTICLE_IMAGES_CREATE);
}
