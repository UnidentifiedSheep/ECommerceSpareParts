using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Common.LRT;

public abstract class LrtNamedObjectBase(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger logger
) : LrtBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger), ILocalizableNamedObject
{
    public abstract string SystemName { get; }
    public abstract string NameLocalizationKey { get; }
    public abstract string DescriptionLocalizationKey { get; }
}