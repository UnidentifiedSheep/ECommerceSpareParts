namespace Abstractions.Models.Validation;

public record ValidationStateData
{
    /// <summary>
    /// Default true.
    /// </summary>
    public bool DisplayErrorToUser { get; init; } = true;

    public object[]? ErrorMessageArguments { get; init; }

    public static ValidationStateData DontDisplay => new() { DisplayErrorToUser = false };
}