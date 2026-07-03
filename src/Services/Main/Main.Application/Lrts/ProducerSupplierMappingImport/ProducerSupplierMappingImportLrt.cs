using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerSupplierMappingImport;

public class ProducerSupplierMappingImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerSupplierMappingImportLrt> logger,
    IPublishEndpoint publisher,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
) 
{
    
    public record ProducerSupplierMappingCsvDto
    {
        [Name("ProducerName")]
        public required string Producer { get; init; }

        [Name("SupplierName", "Supplier")]
        public required Supplier Supplier { get; init; }
        
        [Name("SupplierProducerName", "SupplierProducer")]
        public required string SupplierProducer { get; init; }
    }
}