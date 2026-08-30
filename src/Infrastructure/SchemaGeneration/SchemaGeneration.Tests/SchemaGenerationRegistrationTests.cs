using FluentAssertions;
using Localization.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Extensions;
using SchemaGeneration.Generators;

namespace SchemaGeneration.Tests;

public sealed class SchemaGenerationRegistrationTests
{
	[Fact]
	public void AddSchemaGeneration_ShouldResolveLocalizedGeneratorByDefault()
	{
		var services = new ServiceCollection();
		services.AddScoped<IContextualStringLocalizer>(_ => new StubContextualStringLocalizer(
			new Dictionary<string, string>
			{
				["value.label"] = "Localized value"
			}));
		services.AddSchemaGeneration();

		using var provider = services.BuildServiceProvider(true);
		using var scope = provider.CreateScope();

		var generator = scope.ServiceProvider.GetRequiredService<ISchemaGenerator>();
		var schema = generator.Generate<LocalizedInput>();

		generator.Should().BeOfType<LocalizedSchemaGenerator>();
		schema.Fields.Single().Label.Should().Be("Localized value");
		schema.Fields.Single().LabelKey.Should().BeNull();
	}

	[Fact]
	public void AddSchemaGeneration_ShouldResolveRawGeneratorAsSingleton()
	{
		var services = new ServiceCollection();
		services.AddScoped<IContextualStringLocalizer>(_ =>
			new StubContextualStringLocalizer(new Dictionary<string, string>()));
		services.AddSchemaGeneration();

		using var provider = services.BuildServiceProvider(true);
		var first = provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Raw);
		var second = provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Raw);

		first.Should().BeOfType<ReflectionSchemaGenerator>();
		second.Should().BeSameAs(first);

		var schema = first.Generate<LocalizedInput>();
		schema.Fields.Single().LabelKey.Should().Be("value.label");
		schema.Fields.Single().Label.Should().BeNull();
	}

	[Fact]
	public void AddSchemaGeneration_ShouldNotLeakLocalizedSchemaBetweenScopes()
	{
		var scopeNumber = 0;
		var services = new ServiceCollection();
		services.AddScoped<IContextualStringLocalizer>(_ =>
		{
			var localizedValue = Interlocked.Increment(ref scopeNumber) == 1 ? "First scope" : "Second scope";

			return new StubContextualStringLocalizer(
				new Dictionary<string, string>
				{
					["value.label"] = localizedValue
				});
		});
		services.AddSchemaGeneration();

		using var provider = services.BuildServiceProvider(true);

		using var firstScope = provider.CreateScope();
		var firstSchema = firstScope
			.ServiceProvider
			.GetRequiredService<ISchemaGenerator>()
			.Generate<LocalizedInput>();

		using var secondScope = provider.CreateScope();
		var secondSchema = secondScope
			.ServiceProvider
			.GetRequiredService<ISchemaGenerator>()
			.Generate<LocalizedInput>();

		firstSchema.Fields.Single().Label.Should().Be("First scope");
		secondSchema.Fields.Single().Label.Should().Be("Second scope");
		secondSchema.Should().NotBeSameAs(firstSchema);
	}

	private sealed record LocalizedInput
	{
		[SchemaFieldLabel("value.label")]
		public string? Value { get; init; }
	}
}
