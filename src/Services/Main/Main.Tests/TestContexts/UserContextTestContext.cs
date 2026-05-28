using Abstractions.Interfaces;
using Application.Common.Interfaces.Settings;
using Main.Persistence.Context;
using Test.Common.Stubs;

namespace Tests.TestContexts;

public class UserContextTestContext(
    DContext context,
    ISettingsService settingsService,
    IUserContext userContext
) : GlobalApplicationSettingTestContext(context, settingsService)
{
    public IUserContext UserContext { get; private set; } = null!;
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync(cancellationToken);
        if (userContext is not UserContextMock uc)
            throw new InvalidOperationException(
                "IUserContext is not UserContextMock. For test it must be UserContextMock");

        UserContext = uc;
        
        uc.SetIsAuthenticated(true)
            .SetUserId(SystemUser.Id);
    }
}