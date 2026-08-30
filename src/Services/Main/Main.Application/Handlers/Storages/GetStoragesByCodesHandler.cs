using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Storages;

public record GetStoragesByCodesQuery : IQuery<GetStoragesByCodesResult>
{
    public IReadOnlyList<string> StorageCodes { get; }

    public GetStoragesByCodesQuery(IEnumerable<string> codes)
    {
        StorageCodes = codes.Distinct().ToList();
    }

    public GetStoragesByCodesQuery(string code) : this([code]) { }
}

public record GetStoragesByCodesResult(IReadOnlyList<StorageDto> Storages);

public class GetStoragesByCodesHandler(
    IReadRepository<Storage, string> repository,
    IProjectionProvider<Storage, StorageDto> projection
) : IQueryHandler<GetStoragesByCodesQuery, GetStoragesByCodesResult>
{
    public async Task<GetStoragesByCodesResult> Handle(
        GetStoragesByCodesQuery request,
        CancellationToken cancellationToken)
    {
        return new GetStoragesByCodesResult(
            await repository.Query
                .Where(x => request.StorageCodes.Contains(x.Code))
                .Project(projection)
                .ToListAsync(cancellationToken));
    }
}
