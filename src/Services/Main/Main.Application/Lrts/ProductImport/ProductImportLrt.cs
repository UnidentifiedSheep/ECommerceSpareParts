using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Main.Application.Lrts.ProducerImport;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerImportLrt> logger) : LrtNamedObjectBase(jobRepository, unitOfWork, logger)
{
    protected override Task DoWork()
    {
        throw new NotImplementedException();
    }

    public override Type InputType => typeof(ProductImportInputState);
    public override Type StateType => typeof(ProductImportState);
    public override string SystemName => nameof(ProductImportLrt);
    public override string NameLocalizationKey => "lrt.product.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.import.description";
}
