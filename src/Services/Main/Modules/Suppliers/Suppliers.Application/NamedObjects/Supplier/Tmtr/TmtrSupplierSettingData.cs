using System.Text.Json.Serialization;
using Main.Entities;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Tmtr;

public record TmtrSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("guaranteedDeliveryOffsetDays")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingDeliveryOffsetNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingDeliveryOffsetDescriptionMessage.Key)]
	public int GuaranteedDeliveryOffsetDays { get; init; } = 1;

	[JsonPropertyName("login")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("password")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingPasswordDescriptionMessage.Key)]
	public string? Password { get; init; }
}

public record TmtrSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel(SupplierTmtrSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("guaranteedDeliveryOffsetDays")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingDeliveryOffsetNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingDeliveryOffsetDescriptionMessage.Key)]
	public int GuaranteedDeliveryOffsetDays { get; init; }

	[JsonPropertyName("login")]
	[SchemaFieldLabel(SupplierTmtrSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("hasPassword")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingHasPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingHasPasswordDescriptionMessage.Key)]
	public bool HasPassword { get; init; }
}
