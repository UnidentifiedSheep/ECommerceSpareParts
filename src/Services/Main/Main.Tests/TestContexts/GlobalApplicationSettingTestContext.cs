using Application.Common.Interfaces.Settings;
using Main.Abstractions.Models.Settings;
using Main.Entities.User;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.User;

namespace Tests.TestContexts;

public abstract class GlobalApplicationSettingTestContext(
    DContext context,
    ISettingsService settingsService
) : TestContextBase<DContext>(context)
{
    public User SystemUser { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        SystemUser = await new SystemUserBuilder(Faker)
            .WithUserName("SYSTEM")
            .BuildAndAddToDb(DbContext);

        var setting = new GlobalApplicationSetting(new GlobalApplicationSettingData
        {
            SystemId = SystemUser.Id,
            ImageBucketName = "images",
            ServiceUrl = "https://www.somewebsite.com"
        });

        await settingsService.SetSetting(setting, cancellationToken);
    }
}