using System.Text.Json.Serialization;
using Main.Entities;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Favorit;

public record FavoritSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierFavoritSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("apiKey")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierFavoritSettingApiKeyNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingApiKeyDescriptionMessage.Key)]
	public string? ApiKey { get; init; }
}

public record FavoritSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel(SupplierFavoritSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("hasApiKey")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingHasApiKeyNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingHasApiKeyDescriptionMessage.Key)]
	public bool HasApiKey { get; init; }
}
