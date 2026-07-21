using System.Security.Cryptography;
using System.Text.Json;

namespace Application.Common.Services;

public static class ConfigurationVersionGenerator
{
    public static string Generate(Action<Utf8JsonWriter> writeConfiguration)
    {
        ArgumentNullException.ThrowIfNull(writeConfiguration);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            writeConfiguration(writer);

        return Convert.ToHexString(
            SHA256.HashData(stream.GetBuffer().AsSpan(0, (int)stream.Length)));
    }
}
