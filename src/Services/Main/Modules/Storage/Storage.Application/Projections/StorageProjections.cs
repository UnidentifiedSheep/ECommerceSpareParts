using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class StorageDtoProjectionProvider : ProjectionProviderBase<Storage, StorageDto>
{
	public override Expression<Func<Storage, StorageDto>> Projection { get; } = x => new StorageDto
	{
		Code = x.Code,
		Location = x.Location,
		Description = x.Description,
		Type = x.Type
	};
}
