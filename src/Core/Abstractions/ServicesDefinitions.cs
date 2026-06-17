using Abstractions.Interfaces;

namespace Abstractions;

public static class ServicesDefinitions
{
    public static readonly Analytics Analytics = new();
    public static readonly Main Main = new();
    public static readonly Pricing Pricing = new();
    public static readonly Search Search = new();
}

public class Analytics : IServiceDefinition
{
    public string ServiceName => "analytics";
}

public class Main : IServiceDefinition
{
    public string ServiceName => "main";
}

public class Pricing : IServiceDefinition
{
    public string ServiceName => "pricing";
}

public class Search : IServiceDefinition
{
    public string ServiceName => "search";
}