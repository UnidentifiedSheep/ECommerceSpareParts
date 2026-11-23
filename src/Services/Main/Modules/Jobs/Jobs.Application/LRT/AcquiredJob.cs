using Application.Common.Interfaces.Lrt;
using Domain.CommonEntities.Job;

namespace Main.Modules.Jobs.Application.LRT;

public sealed record AcquiredJob(Job Job, ILrtQuotaLease Lease);
