using Abstractions.Interfaces;

namespace Abstractions;

public static class ServicesDefinitions
{
    public static readonly Analytics Analytics = new();
    public static readonly Main Main = new();
    public static readonly Pricing Pricing = new();
    public static readonly Search Search = new();
}

public sealed class Analytics : IServiceDefinition
{
    public string ServiceName => "analytics";
}

public sealed class Main : IServiceDefinition
{
    public string ServiceName => "main";
}

public sealed class Pricing : IServiceDefinition
{
    public string ServiceName => "pricing";
}

public sealed class Search : IServiceDefinition
{
    public string ServiceName => "search";
}