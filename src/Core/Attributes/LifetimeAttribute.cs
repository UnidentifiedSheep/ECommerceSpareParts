namespace Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class LifetimeAttribute(Lifetime lifetime) : Attribute
{
    public Lifetime Lifetime => lifetime;
}

public enum Lifetime
{
    Singleton,
    Transient,
    Scoped
}