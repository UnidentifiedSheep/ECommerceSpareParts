namespace Analytics.Enums;

[Flags]
public enum DependsOn
{
    None = 0,
    Sale = 1 << 0,
    Purchase = 1 << 1,
    Period = 1 << 2
}