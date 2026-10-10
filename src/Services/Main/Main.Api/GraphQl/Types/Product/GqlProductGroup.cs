using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Product;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("ProductGroup")]
public record GqlProductGroup
{
	private readonly ProductGroupDto? _productGroupDto;

	public GqlProductGroup(int id)
	{
		Id = id;
	}

	public GqlProductGroup(ProductGroupDto dto) : this(dto.Id)
	{
		_productGroupDto = dto;
	}

	[GraphQLName("id")]
	[Shareable]
	public int Id { get; }

	[GraphQLName("name")]
	public async Task<string> GetNameAsync(
		IProductGroupByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetProductGroupAsync(loader, cancellationToken)).Name;

	[GraphQLName("normalizedName")]
	public async Task<string> GetNormalizedNameAsync(
		IProductGroupByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetProductGroupAsync(loader, cancellationToken)).NormalizedName;

	private async Task<ProductGroupDto> GetProductGroupAsync(
		IProductGroupByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_productGroupDto ?? await loader.LoadAsync(Id, cancellationToken) ??
		throw new ProductGroupNotFoundException(Id);
}
