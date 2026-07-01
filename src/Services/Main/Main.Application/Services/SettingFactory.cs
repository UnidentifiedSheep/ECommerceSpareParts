using Application.Common.Services.Settings;
using Main.Entities.Settings;

namespace Main.Application.Services;

public class SettingFactory : SettingFactoryBase
{
    public SettingFactory()
    {
        Register(json => new CurrencySetting(json));
        Register(json => new GlobalApplicationSetting(json));
        Register(json => new StorageContentSetting(json));
    }
}