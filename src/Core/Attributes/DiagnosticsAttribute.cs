namespace Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DiagnosticsAttribute : Attribute
{
    public DiagnosticsAttribute(
        bool enabled = true,
        long maxExecutionTimeMs = 3000)
    {
        Enabled = enabled;
        MaxExecutionTimeMs = maxExecutionTimeMs;
    }

    public bool Enabled { get; }
    public long MaxExecutionTimeMs { get; }
}