using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class MapCatalogueCandidatesToProductsLrt(
    IRepository<Job, Guid> jobRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publisher,
    ILogger logger) : LrtBase(
    jobRepository, 
    unitOfWork,
    publisher, 
    logger)
{
    public const string LrtSystemName = nameof(MapCatalogueCandidatesToProductsLrt);
}