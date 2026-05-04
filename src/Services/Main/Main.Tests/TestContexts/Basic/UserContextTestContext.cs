using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Stubs;
using Tests.TestContexts.Base;

namespace Tests.TestContexts.Basic;

public class UserContextTestContext(
    DContext context, 
    IMediator mediator,
    ISettingsService settingsService,
    IUserContext userContext
    ) : GlobalApplicationSettingTestContext(context, mediator, settingsService)
{
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync(cancellationToken);
        if (userContext is not UserContextMock uc)
            throw new InvalidOperationException("IUserContext is not UserContextMock. For test it must be UserContextMock");
        
        uc.SetIsAuthenticated(true)
            .SetUserId(SystemUser.Id);
    }
}