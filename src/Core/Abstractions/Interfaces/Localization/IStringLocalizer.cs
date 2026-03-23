namespace Abstractions.Interfaces.Localization;

public interface IStringLocalizer
{
    string Get(string key, string locale);
}