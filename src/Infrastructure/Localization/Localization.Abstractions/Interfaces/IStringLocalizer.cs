using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface IStringLocalizer
{
    string Get(string key, Locale locale);

    string Get(
        string key,
        Locale locale,
        params object[] arguments);

    bool TryGet(
        string key,
        Locale locale,
        out string? value);

    bool TryGet(
        string key,
        Locale locale,
        out string? value,
        params object[] arguments);
}