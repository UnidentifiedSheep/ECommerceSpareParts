namespace Attributes;

/// <summary>
/// Saves changes automatically after the command handler completes successfully.
/// Automatic saving can be suppressed for the current unit of work.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoSaveAttribute : Attribute;
