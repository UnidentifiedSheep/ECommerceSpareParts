using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Modules.Product.Application.Commands.Products;
using Main.Modules.Product.Application.Commands.Products.AddProductContent;
using Main.Modules.Product.Application.Commands.Products.CreateProducts;
using Main.Modules.Product.Application.Commands.Products.MakeLinkageBetweenArticles;
using Main.Modules.Product.Application.Commands.Products.PatchProduct;
using Main.Modules.Product.Application.Commands.Products.RemoveProductContent;
using Main.Modules.Product.Application.Commands.Products.SetProductContentQuantity;
using Main.Modules.Product.Application.Commands.Products.SetProductSizes;
using Main.Modules.Product.Application.Commands.Products.SetProductWeight;
using Main.Modules.Product.Application.Commands.Products.UpsertCharacteristics;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class ProductMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_CREATE)]
	public async Task<IReadOnlyList<GqlProduct>> CreateAsync(
		ISender sender,
		IReadOnlyList<GqlCreateProductInput> products,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateProductsCommand(products.Select(product => product.ToDto()).ToList()),
			cancellationToken);

		return result.CreatedIds.Select(id => new GqlProduct(id)).ToArray();
	}

	[GraphQLName("patch")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_EDIT)]
	public async Task<GqlProduct> PatchAsync(
		ISender sender,
		int productId,
		GqlPatchProductInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new PatchProductCommand(productId, input.ToDto()),
			cancellationToken);

		return new GqlProduct(productId);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_DELETE)]
	public async Task<bool> DeleteAsync(
		ISender sender,
		GqlDeleteProductInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new DeleteProductCommand(input.ProductId, input.RowVersion),
			cancellationToken);
		return true;
	}

	[GraphQLName("setSize")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_SIZES_CREATE)]
	public async Task<GqlProduct> SetSizeAsync(
		GqlSetProductSizeInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new SetProductSizesCommand(
				input.ProductId,
				input.Length,
				input.Width,
				input.Height,
				input.Unit),
			cancellationToken);
		return new GqlProduct(input.ProductId);
	}

	[GraphQLName("deleteSize")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_SIZES_DELETE)]
	public async Task<GqlProduct> DeleteSizeAsync(
		int productId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteArticleSizesCommand(productId), cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("setWeight")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_WEIGHT_CREATE)]
	public async Task<GqlProduct> SetWeightAsync(
		GqlSetProductWeightInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new SetProductWeightCommand(
				input.ProductId,
				input.Weight,
				input.Unit),
			cancellationToken);
		return new GqlProduct(input.ProductId);
	}

	[GraphQLName("deleteWeight")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_WEIGHT_DELETE)]
	public async Task<GqlProduct> DeleteWeightAsync(
		int productId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteProductWeightCommand(productId), cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("upsertCharacteristics")]
	[RequireAllPermissions(
		PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE,
		PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE)]
	public async Task<bool> UpsertCharacteristicsAsync(
		IReadOnlyCollection<GqlUpsertProductCharacteristicInput> items,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new UpsertCharacteristicsCommand(items.Select(item => item.ToDto()).ToArray()),
			cancellationToken);
		return true;
	}

	[GraphQLName("deleteCharacteristic")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE)]
	public async Task<GqlProduct> DeleteCharacteristicAsync(
		int productId,
		int characteristicId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteCharacteristicsCommand(productId, characteristicId), cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("addContent")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_CONTENT_CREATE)]
	public async Task<GqlProduct> AddContentAsync(
		int productId,
		IReadOnlyCollection<GqlProductContentItemInput> items,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddProductContentCommand(
				productId,
				items.ToDictionary(item => item.ChildProductId, item => item.Quantity)),
			cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("removeContent")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_CONTENT_DELETE)]
	public async Task<GqlProduct> RemoveContentAsync(
		int productId,
		int childProductId,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RemoveProductContentCommand(productId, childProductId), cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("setContentCount")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_CONTENT_EDIT)]
	public async Task<GqlProduct> SetContentCountAsync(
		int productId,
		int childProductId,
		int quantity,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new SetProductsContentCountCommand(
				productId,
				childProductId,
				quantity),
			cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("removeImage")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_IMAGES_DELETE)]
	public async Task<GqlProduct> RemoveImageAsync(
		int productId,
		string imagePath,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RemoveProductImageCommand(productId, imagePath), cancellationToken);
		return new GqlProduct(productId);
	}

	[GraphQLName("createCrosses")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_CROSSES_CREATE)]
	public async Task<bool> CreateCrossesAsync(
		IReadOnlyCollection<GqlProductLinkageInput> linkages,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new MakeLinkageBetweenProductsCommand(linkages.Select(linkage => linkage.ToDto()).ToArray()),
			cancellationToken);
		return true;
	}
}
