using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;

namespace Pricing.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class MarkupGroupDtoProjectionProvider : ProjectionProviderBase<MarkupGroup, MarkupGroupDto>
{
	public override Expression<Func<MarkupGroup, MarkupGroupDto>> Projection { get; } = group => new MarkupGroupDto
	{
		Id = group.Id,
		Name = group.Name,
		CurrencyId = group.CurrencyId,
		SourceMessageId = group.SourceMessageId
	};
}

[Lifetime(Lifetime.Singleton)]
public sealed class MarkupRangeDtoProjectionProvider : ProjectionProviderBase<MarkupRange, MarkupRangeDto>
{
	public override Expression<Func<MarkupRange, MarkupRangeDto>> Projection { get; } = range => new MarkupRangeDto
	{
		Id = range.Id,
		RangeStart = range.RangeStart,
		RangeEnd = range.RangeEnd,
		Markup = range.Markup
	};
}
