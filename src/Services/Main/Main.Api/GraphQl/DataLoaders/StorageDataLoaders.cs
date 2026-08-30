using GreenDonut;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Storages;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class StorageDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<string, StorageDto>>
        GetStorageByCode(
            IReadOnlyList<string> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetStoragesByCodesQuery(keys),
                cancellationToken))
            .Storages
            .ToDictionary(
                x => x.Code,
                x => x);
    }
}
