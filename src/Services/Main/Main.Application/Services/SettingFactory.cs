using Application.Common.Services.Settings;
using Main.Abstractions.Models.Settings;

namespace Main.Application.Services;

public class SettingFactory : SettingFactoryBase
{
    public SettingFactory()
    {
        Register(json => new CurrencySetting(json));
    }
}