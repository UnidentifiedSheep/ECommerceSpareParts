using Abstractions.Interfaces;
using Integrations.Common;
using Internal.Integration.Core.Models.Common;

namespace Internal.Integration.Core.Interfaces.Common;

public interface IJobNode
{
    Task<Response<IReadOnlyList<InternalJobInfo>>> GetAvailableJobs(
        IServiceDefinition serviceDefinition,
        string? locale,
        CancellationToken cancellationToken = default);
}