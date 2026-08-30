using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Search.Application.Dtos.Producers;
using Search.Entities;
using ProducerAlias = Search.Entities.ProducerAlias;
using ProducerAliasDto = Search.Application.Dtos.Producers.ProducerAlias;

namespace Search.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerSearchDtoProjectionProvider : ProjectionProviderBase<Producer, ProducerSearchDto>
{
	public override Expression<Func<Producer, ProducerSearchDto>> Projection { get; } = producer =>
		new ProducerSearchDto
		{
			Id = producer.Id,
			Name = producer.Name,
			Description = producer.Description
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	ProducerAliasDtoProjectionProvider : ProjectionProviderBase<ProducerAlias, ProducerAliasDto>
{
	public override Expression<Func<ProducerAlias, ProducerAliasDto>> Projection { get; } = alias =>
		new ProducerAliasDto
		{
			Alias = alias.Alias
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProducerDtoProjectionProvider : ProjectionProviderBase<Producer, ProducerDto>
{
	public ProducerDtoProjectionProvider(IProjectionProvider<ProducerAlias, ProducerAliasDto> aliasProjection)
	{
		var aliasToDto = aliasProjection.Projection;

		Projection = producer => new ProducerDto
		{
			Id = producer.Id,
			Name = producer.Name,
			Description = producer.Description,
			Aliases = producer.Aliases.Select(x => aliasToDto.Invoke(x))
		};
	}

	public override Expression<Func<Producer, ProducerDto>> Projection { get; }
}
