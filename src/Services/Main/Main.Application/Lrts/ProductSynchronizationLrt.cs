using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Products;
using Domain.CommonEntities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public sealed class ProductSynchronizationLrt(
    IRepository<Job, Guid> jobRepository,
    IReadRepository<Product, int> productRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger<ProductSynchronizationLrt> logger
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{

    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    public override Type InputType => typeof(NoneInputState);
    public override Type StateType => typeof(NoneInputState);
    public override string SystemName => nameof(ProductSynchronizationLrt);
    public override string NameLocalizationKey => "lrt.product.synchronization.name";
    public override string DescriptionLocalizationKey => "lrt.product.synchronization.description";
    protected override async Task DoWork()
    {
        int lastId = -1;
        const int batchSize = 1000;

        while (true)
        {
            var ids = await GetIdsAsync(lastId, batchSize);
            if (ids.Count == 0) break;
            
            lastId = ids[^1];
            await PublishEventsAsync(ids);

            if (ids.Count < batchSize) break;
        }
    }

    private async Task<IReadOnlyList<int>> GetIdsAsync(int lastId, int batchSize)
        => await productRepository.Query
            .Where(x => x.Id > lastId)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .Take(batchSize)
            .ToListAsync(CancellationToken);

    private Task PublishEventsAsync(IReadOnlyList<int> ids)
        => UnitOfWork.ExecuteWithTransaction(
            settings: TransactionalAttribute.ReadCommited(20, 2),
            action: async () =>
            {
                foreach (var id in ids)
                    await Publisher.Publish(new ProductUpdatedEvent
                    {
                        Id = id
                    });
                
                await UnitOfWork.SaveChangesAsync(CancellationToken);
            },
            cancellationToken: CancellationToken);
}
