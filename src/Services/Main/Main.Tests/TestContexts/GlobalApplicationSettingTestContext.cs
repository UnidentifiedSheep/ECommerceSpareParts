using Application.Common.Interfaces.Settings;
using Main.Entities.Settings;
using Main.Entities.User;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Stubs;

namespace Tests.TestContexts;

public abstract class GlobalApplicationSettingTestContext(
    DContext context,
    ISettingsService settingsService,
    TestSystemOptionsAccessor systemOptionsAccessor
) : TestContextBase<DContext>(context)
{
    public User SystemUser { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        SystemUser = await new SystemUserBuilder(Faker)
            .WithUserName("SYSTEM")
            .BuildAndAddToDb(DbContext);
        systemOptionsAccessor.SystemId = SystemUser.Id;

        var setting = new GlobalApplicationSetting(
            new GlobalApplicationSettingData
            {
                S3ServiceUrl = "https://www.somewebsite.com",
                ApiServiceUrl = "https://www.somewebsite.com",
                AppServiceUrl = "https://www.somewebsite.com"
            });

        await settingsService.SetSetting(setting, cancellationToken);
    }
}