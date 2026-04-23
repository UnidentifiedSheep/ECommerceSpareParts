using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.Interfaces.Settings;

public interface ISettingFactory
{
    Setting Create(string key, string json); 
    Setting Create<T>(string json) where T : Setting, ISetting<T>;
    void Register<T>(Func<string, T> factory) where T : Setting, ISetting<T>;
}