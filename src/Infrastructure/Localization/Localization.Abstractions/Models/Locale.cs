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

    public static implicit operator Locale(string locale) => new(locale);

    public static implicit operator string(Locale locale) => locale._locale;

    public static bool operator ==(Locale left, Locale right) => left._locale == right._locale;
    public static bool operator !=(Locale left, Locale right) => left._locale != right._locale;
    
    public bool Equals(Locale other) => _locale == other._locale;
    public override bool Equals(object? obj) => obj is Locale other && Equals(other);
    public override int GetHashCode() => _locale.GetHashCode();
    
    public override string ToString() => _locale;

    private static string NormalizeLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) throw new ArgumentNullException(nameof(locale)); 
        locale = locale.Trim().Replace('_', '-'); 
        return locale.Split('-')[0].ToUpperInvariant(); 
        
    }
}