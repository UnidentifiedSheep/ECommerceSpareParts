using Analytics.Entities.Settings;
using Application.Common.Services.Settings;

namespace Analytics.Application.Services;

public class SettingFactory : SettingFactoryBase
{
    public SettingFactory()
    {
        Register(json => new GlobalApplicationSetting(json));
    }
}