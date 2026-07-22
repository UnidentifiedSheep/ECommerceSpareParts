using Application.Common.Interfaces.Settings;
using Main.Entities.Settings;
using Main.Entities.User;
using Main.Entities.Organization;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.User;
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
        SystemUser = new SystemUserBuilder(Faker)
            .WithUserName("SYSTEM")
            .Build();
        var systemOrganization = Organization.CreateSystem(SystemUser.Id, SystemUser.Id);
        var financialProfile = OrganizationFinancialProfile.Create(SystemUser.Id, decimal.MinValue);
        await DbContext.AddRangeAsync(SystemUser, systemOrganization, financialProfile);
        await DbContext.SaveChangesAsync(cancellationToken);
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
