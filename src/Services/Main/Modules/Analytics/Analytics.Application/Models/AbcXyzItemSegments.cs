using System.Text.Json.Serialization;
using Main.Modules.Analytics.Abstractions.Models;

namespace Main.Modules.Analytics.Application.Models;

public sealed record AbcXyzItemSegments(
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AbcClass Abc,
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	XyzClass? Xyz);
