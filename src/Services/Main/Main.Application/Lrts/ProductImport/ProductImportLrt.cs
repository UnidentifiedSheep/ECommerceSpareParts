using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Main.Application.Lrts.ProducerImport;
using Main.Entities.Producer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
    IRepository<Job, Guid> jobRepository,
    IReadRepository<Producer, int> producerReadRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerImportLrt> logger) : LrtNamedObjectBase(jobRepository, unitOfWork, logger)
{

    public override Type InputType => typeof(ProductImportInputState);
    public override Type StateType => typeof(ProductImportState);
    public override string SystemName => nameof(ProductImportLrt);
    public override string NameLocalizationKey => "lrt.product.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.import.description";
    
    private readonly Dictionary<string, int> _producerNamesToIds = new();
    private readonly Dictionary<string, int> _otherNamesToIds = new();
    
    protected override Task DoWork()
    {
        throw new NotImplementedException();
    }

    private async Task LoadProducers()
    {
        
    }
}
