using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Main.Abstractions.Models.Settings;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.User;

namespace Tests.TestContexts.Base;

public abstract class GlobalApplicationSettingTestContext(
    DContext context, 
    IMediator mediator, 
    ISettingsService settingsService,
    IUnitOfWork unitOfWork)
    : TestContextBase<DContext>(context, mediator)
{
    protected IUnitOfWork UnitOfWork => unitOfWork;
    protected ISettingsService SettingsService => settingsService;
    
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var user = await new SystemUserBuilder(Faker)
            .WithUserName("SYSTEM")
            .BuildAndAddToDb(unitOfWork);

        var setting = new GlobalApplicationSetting(new GlobalApplicationSettingData
        {
            SystemId = user.Id,
            ImageBucketName = "images",
            ServiceUrl = "https://www.somewebsite.com",
        });
        
        await settingsService.SetSetting(setting, cancellationToken);
    }
}