using Application.Common.Services.Settings;
using Main.Entities.Settings;
using Main.Entities.Settings.Supplier;

namespace Main.Application.Services;

public class SettingFactory : SettingFactoryBase
{
    public SettingFactory()
    {
        Register(json => new CurrencySetting(json));
        Register(json => new GlobalApplicationSetting(json));
        Register(json => new StorageContentSetting(json));
        Register(json => new FavoritSupplierSetting(json));
    }
}