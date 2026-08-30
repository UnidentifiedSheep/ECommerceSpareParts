namespace Tests.Interfaces;

public interface IDependentTestContext
{
	static abstract Type[] DependsOn { get; }
}
