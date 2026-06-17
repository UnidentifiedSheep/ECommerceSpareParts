using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.NamedObject;

public abstract class LrtNamedObjectBase(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger logger
    ) : LrtBase(jobRepository, unitOfWork, publisher, logger), ILocalizableNamedObject
{
    public abstract string SystemName { get; }
    public abstract string NameLocalizationKey { get; }
    public abstract string DescriptionLocalizationKey { get; }
}