using System.Text;

namespace Search.Entities;

public static class ProductSkuNormalizer
{
    public static string Normalize(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return string.Empty;
        }

        var normalized = new StringBuilder(sku.Length);

        foreach (var ch in sku)
        {
            if (char.IsLetterOrDigit(ch))
            {
                normalized.Append(char.ToLowerInvariant(ch));
            }
        }

        return normalized.ToString();
    }
}
