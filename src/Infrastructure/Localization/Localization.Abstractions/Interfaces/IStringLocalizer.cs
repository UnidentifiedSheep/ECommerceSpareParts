using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface IStringLocalizer
{
    string Get(string key, Locale locale);
}