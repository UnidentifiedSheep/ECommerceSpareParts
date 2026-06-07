using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities;

namespace Application.Common.NamedObject;

public abstract class LrtNamedObjectBase(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork
    ) : LrtBase(jobRepository, unitOfWork), ILocalizableNamedObject
{
    public abstract string SystemName { get; }
    public abstract string NameLocalizationKey { get; }
    public abstract string DescriptionLocalizationKey { get; }
}