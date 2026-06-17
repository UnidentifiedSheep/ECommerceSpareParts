using Abstractions.Interfaces;
using Internal.Integration.Core.Models.Common;

namespace Internal.Integration.Core.Interfaces;

public interface ICommonClient
{
    Task<IReadOnlyList<InternalJobInfo>> GetAvailableJobs(
        IServiceDefinition serviceDefinition,
        string? locale,
        CancellationToken cancellationToken = default);
}