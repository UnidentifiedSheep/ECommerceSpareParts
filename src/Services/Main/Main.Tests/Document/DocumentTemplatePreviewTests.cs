using System.Text.Json;
using Enums;
using FluentAssertions;
using Main.Application.Document.Interfaces.Document;
using Main.Modules.Documents.Application.Services.Document;
using Main.Modules.Documents.Application.Services.Document.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Document;

public sealed class DocumentTemplatePreviewTests : ServiceTest
{
	private static readonly string[] Cultures = ["en", "ru", "tr"];

	private static readonly string RepositoryRoot = FindRepositoryRoot();

	private static readonly string PreviewDirectory = Path.Combine(RepositoryRoot, ".document-preview");

	private static readonly string SamplesDirectory = Path.Combine(
		RepositoryRoot,
		"src",
		"Services",
		"Main",
		"Main.Tests",
		"Document",
		"Samples");

	public static TheoryData<string, string> Samples
	{
		get
		{
			var data = new TheoryData<string, string>();
			foreach (var sample in Directory.EnumerateFiles(SamplesDirectory, "*.json").Order())
			foreach (var culture in Cultures)
				data.Add(Path.GetFileNameWithoutExtension(sample), culture);
			return data;
		}
	}

	[Theory]
	[MemberData(nameof(Samples))]
	public async Task Template_RendersSampleWithoutErrors(string systemName, string culture)
	{
		var definition = CreateDefinition(systemName);
		var schema = await ReadSampleAsync(systemName, definition.SchemaType);

		using var output = new MemoryStream();
		await definition.RenderAsync(schema, DocumentType.Excel, culture, output, CancellationToken);

		output.Length.Should().BePositive();
		await SavePreviewAsync(systemName, culture, output);
	}

	private IDocumentDefinition CreateDefinition(string systemName)
	{
		var resolver = new DocumentTemplateResolver(
			Services.GetServices<IDocumentProvider>(),
			[new EmbeddedDocumentTemplateSource()]);

		return DefinitionTypes
			.Select(type => (IDocumentDefinition)ActivatorUtilities.CreateInstance(Services, type, resolver))
			.Single(definition => definition.SystemName == systemName);
	}

	private static IEnumerable<Type> DefinitionTypes
		=> AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(assembly => assembly.GetName().Name?.StartsWith("Main.Modules.") == true)
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type is { IsClass: true, IsAbstract: false } &&
				typeof(IDocumentDefinition).IsAssignableFrom(type));

	private async Task<object> ReadSampleAsync(string systemName, Type schemaType)
	{
		await using var stream = File.OpenRead(Path.Combine(SamplesDirectory, $"{systemName}.json"));
		return await JsonSerializer.DeserializeAsync(stream, schemaType, cancellationToken: CancellationToken)
			?? throw new InvalidOperationException($"Sample for '{systemName}' is empty.");
	}

	private async Task SavePreviewAsync(string systemName, string culture, MemoryStream output)
	{
		Directory.CreateDirectory(PreviewDirectory);
		output.Position = 0;
		await using var file = File.Create(Path.Combine(PreviewDirectory, $"{systemName}.{culture}.xlsx"));
		await output.CopyToAsync(file, CancellationToken);
	}

	private static string FindRepositoryRoot()
	{
		for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
			directory is not null;
			directory = directory.Parent)
			if (File.Exists(Path.Combine(directory.FullName, "ECommerceSpareParts.sln")))
				return directory.FullName;

		throw new InvalidOperationException("Repository root with ECommerceSpareParts.sln was not found.");
	}
}
