using Armtek.Integrations.Core.Models;

namespace Armtek.Integrations.Core.Interfaces;

public interface IArmtekConnectionProvider
{
    Task<ArmtekConnectionModel> GetConnectionAsync(CancellationToken ct);
}