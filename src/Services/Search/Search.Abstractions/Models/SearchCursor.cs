using System.Text;
using System.Text.Json;

namespace Search.Abstractions.Models;

public sealed class SearchCursor
{
    public float Score
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    public int DocId
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }
    //Can be negative.
    public int ShardIndex { get; init; }
    
    public static string EncodeCursor(SearchCursor cursor)
    {
        string json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
    
    public string EncodeCursor() => EncodeCursor(this);

    /// <summary>
    /// Decodes a base64-encoded cursor string into a <see cref="SearchCursor"/> object.
    /// </summary>
    /// <param name="cursor">
    /// The base64-encoded string representation of the <see cref="SearchCursor"/>.
    /// If the value is null, empty, or whitespace, the method returns null.
    /// </param>
    /// <returns>
    /// A <see cref="SearchCursor"/> object if the decoding is successful; otherwise, null
    /// if the input is invalid or deserialization fails.
    /// </returns>
    public static SearchCursor? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return null;
        string json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        try
        {
            return JsonSerializer.Deserialize<SearchCursor>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }
}