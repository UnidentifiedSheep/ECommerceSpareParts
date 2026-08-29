using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Enums;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Storage")]
public record GqlStorage
{
    private StorageDto? _storage;
    
    [GraphQLName("code")]
    public string Code { get; }

    [GraphQLName("description")]
    public async Task<string?> GetDescriptionAsync(
        IStorageByCodeDataLoader loader,
        CancellationToken token)
        => (await GetStorageAsync(loader, token)).Description;

    [GraphQLName("location")]
    public async Task<string?> GetLocationAsync(
        IStorageByCodeDataLoader loader,
        CancellationToken token)
        => (await GetStorageAsync(loader, token)).Location;


    [GraphQLName("type")]
    public async Task<StorageType> GetTypeAsync(
        IStorageByCodeDataLoader loader,
        CancellationToken token)
        => (await GetStorageAsync(loader, token)).Type;
    
    private async Task<StorageDto> GetStorageAsync(
        IStorageByCodeDataLoader loader,
        CancellationToken token)
    {
        return _storage ??= await loader.LoadAsync(Code, token)
            ?? throw new StorageNotFoundException(Code);
    }
    
    public GqlStorage(string code)
    {
        Code = code;
    }

    public GqlStorage(StorageDto storage) : this(storage.Code)
    {
        _storage = storage;
    }
}