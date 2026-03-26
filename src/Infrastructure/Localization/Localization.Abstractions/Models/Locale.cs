namespace Localization.Abstractions.Models;

public readonly struct Locale : IEquatable<Locale>
{
    private readonly string _locale;

    public Locale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentNullException(nameof(locale));

        _locale = NormalizeLocale(locale);
    }

    public static implicit operator Locale(string locale)
    {
        return new Locale(locale);
    }

    public static implicit operator string(Locale locale)
    {
        return locale._locale;
    }

    public static bool operator ==(Locale left, Locale right)
    {
        return left._locale == right._locale;
    }

    public static bool operator !=(Locale left, Locale right)
    {
        return left._locale != right._locale;
    }

    public bool Equals(Locale other)
    {
        return _locale == other._locale;
    }

    public override bool Equals(object? obj)
    {
        return obj is Locale other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _locale.GetHashCode();
    }

    public override string ToString()
    {
        return _locale;
    }

    private static string NormalizeLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) throw new ArgumentNullException(nameof(locale));
        locale = locale.Trim().Replace('_', '-');
        return locale.Split('-')[0].ToUpperInvariant();
    }
}