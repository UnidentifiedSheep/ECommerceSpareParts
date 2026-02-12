namespace Abstractions.Interfaces.Currency;

public interface ICurrencyConverterSetup
{
    /// <summary>
    /// Initialize currency converter
    /// </summary>
    Task InitializeAsync();
}