using Analytics.Enums;

namespace Analytics.Entities.Interfaces;

public interface IDependency
{
    public static abstract DependsOn DependsOn { get; }
}