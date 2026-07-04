using Armtek.Integrations.Core.Interfaces;

namespace Armtek.Integrations.Client;

public class ArmtekClient(IArmtekConnectionProvider connectionProvider) : IArmtekClient
{
    public IUserSettingsNode UserSettingsNode => new UserSettingsNode(connectionProvider);
}