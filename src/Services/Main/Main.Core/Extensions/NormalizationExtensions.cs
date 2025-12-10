using System.Text.RegularExpressions;

namespace Main.Core.Extensions;

public static partial class NormalizationExtensions
{
    /// <summary>
    ///     Возвращает нормализованый артикул те состоящий только из букв и цифр.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToNormalizedArticleNumber(this string source)
    {
        return global::Core.Extensions.NormalizationExtensions.OnlyCharacter().Replace(source, "").ToUpperInvariant();
    }

    public static string ToNormalized(this string source)
    {
        return source.Trim().ToUpperInvariant();
    }
}