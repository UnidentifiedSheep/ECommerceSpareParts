using System.Text.Json.Serialization;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models;

public sealed record AbcXyzItemSegments(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.segments.abc.name")]
	[property: SchemaFieldDescription("abc.xyz.segments.abc.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AbcClass Abc,
	[property: SchemaFieldLabel("abc.xyz.segments.xyz.name")]
	[property: SchemaFieldDescription("abc.xyz.segments.xyz.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	XyzClass? Xyz);
