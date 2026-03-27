namespace Analytics.Enums;

[Flags]
public enum RecalculationTags
{
    None = 0,
    RecalculationNeeded = 1 << 0,
    Disabled = 1 << 1
}