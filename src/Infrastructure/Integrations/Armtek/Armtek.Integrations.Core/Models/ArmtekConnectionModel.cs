using Integrations.Common;

namespace Armtek.Integrations.Core.Models;

public record ArmtekConnectionModel
{
    public required Uri BaseUri { get; init; }
    public required BasicAuthModel Auth { get; init; }
}