using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;
using MediatR;
using Pricing.Application.Handlers.Markup.GetMarkupRanges;
using Pricing.Api.GraphQl.DataLoaders;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities.Exceptions.Markup;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("MarkupGroup")]
public class GqlMarkupGroup
{
	private MarkupGroupDto? _dto;

	[GraphQLName("id")]
	public int Id { get; }

	[GraphQLName("name")]
	public async Task<string?> Name(
		IMarkupGroupByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetGroupAsync(loader, cancellationToken)).Name;

	[GraphQLName("currency")]
	public async Task<GqlCurrency> Currency(
		IMarkupGroupByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> new((await GetGroupAsync(loader, cancellationToken)).CurrencyId);

	[GraphQLName("sourceMessageId")]
	public async Task<Guid?> SourceMessageId(
		IMarkupGroupByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetGroupAsync(loader, cancellationToken)).SourceMessageId;

	[GraphQLName("ranges")]
	[RequireAllPermissions(PermissionCodes.MARKUP_GET)]
	public async Task<IReadOnlyList<GqlMarkupRange>> GetRangesAsync(
		GqlPagination pagination,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetMarkupRangesQuery(Id, pagination), cancellationToken);
		return result.Ranges.Select(range => new GqlMarkupRange(range)).ToArray();
	}

	public GqlMarkupGroup(int id)
	{
		Id = id;
	}

	public GqlMarkupGroup(MarkupGroupDto dto)
	{
		_dto = dto;
		Id = _dto.Id;
	}


	private async Task<MarkupGroupDto> GetGroupAsync(
		IMarkupGroupByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ??= await loader.LoadAsync(Id, cancellationToken)
			?? throw new MarkupGroupNotFoundException(Id);
}
