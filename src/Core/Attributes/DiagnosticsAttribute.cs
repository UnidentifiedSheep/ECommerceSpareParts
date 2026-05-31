namespace Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DiagnosticsAttribute : Attribute
{
    public bool Enabled { get; }
    public long MaxExecutionTimeMs { get; }
    
    public DiagnosticsAttribute(
        bool enabled = true,
        long maxExecutionTimeMs = 3000)
    {
        Enabled = enabled;
        MaxExecutionTimeMs = maxExecutionTimeMs;
    }
}