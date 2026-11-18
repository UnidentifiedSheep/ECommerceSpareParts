using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record ChartReference(SchemaEntity Entity, string Field, object? Key);
