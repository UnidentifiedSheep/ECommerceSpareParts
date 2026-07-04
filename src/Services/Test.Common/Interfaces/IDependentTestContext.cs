namespace Tests.Interfaces;

public interface IDependentTestContext
{
    public static abstract Type[] DependsOn { get; }
}