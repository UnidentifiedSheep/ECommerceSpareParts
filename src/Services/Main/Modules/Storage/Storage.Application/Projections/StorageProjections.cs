using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Modules.Storage.Application.Dtos.Storage;

namespace Main.Modules.Storage.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class StorageDtoProjectionProvider : ProjectionProviderBase<Entities.Storage.Storage.Storage, StorageDto>
{
	public override Expression<Func<Entities.Storage.Storage.Storage, StorageDto>> Projection { get; } = x => new StorageDto
	{
		Code = x.Code,
		Location = x.Location,
		Description = x.Description,
		Type = x.Type
	};
}
