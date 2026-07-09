using Application.Common.Services.Settings;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services;

public class SettingFactory : SettingFactoryBase
{
    public SettingFactory()
    {
        Register(json => new PricingSetting(json));
    }
}