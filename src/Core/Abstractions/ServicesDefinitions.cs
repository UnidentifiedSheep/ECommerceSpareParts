using Abstractions.Interfaces;

namespace Abstractions;

public static class ServicesDefinitions
{
	public static readonly Main Main = new();

	public static readonly Search Search = new();

	public static readonly Gateway Gateway = new();
}

public sealed class Main : IServiceDefinition
{
	public string ServiceName => "main";
}

public sealed class Search : IServiceDefinition
{
	public string ServiceName => "search";
}

public sealed class Gateway : IServiceDefinition
{
	public string ServiceName => "gateway";
}
