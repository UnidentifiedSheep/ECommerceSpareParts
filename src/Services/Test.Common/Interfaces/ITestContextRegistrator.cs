namespace Test.Common.Interfaces;

public interface ITestContextRegistrator
{
    public static abstract Type[] DependsOn { get; }
}