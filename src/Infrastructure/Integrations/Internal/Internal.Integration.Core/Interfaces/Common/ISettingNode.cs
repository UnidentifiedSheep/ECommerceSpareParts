using Abstractions.Interfaces;
using Integrations.Common;

namespace Internal.Integration.Core.Interfaces.Common;

public interface ISettingNode
{
    Task<Response<string>> GetSetting(
        IServiceDefinition serviceDefinition,
        string systemName,
        CancellationToken cancellationToken = default);
}