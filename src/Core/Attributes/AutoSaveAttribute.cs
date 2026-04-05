namespace Attributes;

/// <summary>
/// Marks if command at the end of command should be executed savechanges automatically
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoSaveAttribute : Attribute
{
    
}