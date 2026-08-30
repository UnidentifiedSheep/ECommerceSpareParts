using System.Text.Json;
using FluentAssertions;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Tests;

public sealed class SchemaContractSerializationTests
{
	[Fact]
	public void ObjectSchema_ShouldSerializeUsingExpectedApiContract()
	{
		var schema = new ObjectSchema
		{
			Fields =
			[
				new FieldSchema
				{
					Name = "productId",
					Type = SchemaValueType.String,
					LabelKey = "product.label",
					Required = true,
					Control = InputControlType.EntitySelector,
					Accepts = ["application/json"],
					Dependency = new SchemaDependency
					{
						EntityName = "Product", FieldName = "id"
					}
				}
			]
		};

		var json = JsonSerializer.SerializeToElement(schema, JsonSerializerOptions.Web);

		json.GetProperty("version").GetInt32().Should().Be(1);
		json.TryGetProperty("fields", out var fields).Should().BeTrue();
		json.TryGetProperty("csvSchema", out _).Should().BeTrue();

		var field = fields.EnumerateArray().Single();
		field.GetProperty("name").GetString().Should().Be("productId");
		field.GetProperty("type").GetInt32().Should().Be((int)SchemaValueType.String);
		field.GetProperty("labelKey").GetString().Should().Be("product.label");
		field.GetProperty("required").GetBoolean().Should().BeTrue();
		field.GetProperty("control").GetInt32().Should().Be((int)InputControlType.EntitySelector);
		field.GetProperty("accepts").EnumerateArray().Single().GetString().Should().Be("application/json");
		field.GetProperty("dependency").GetProperty("entityName").GetString().Should().Be("Product");
	}
}
