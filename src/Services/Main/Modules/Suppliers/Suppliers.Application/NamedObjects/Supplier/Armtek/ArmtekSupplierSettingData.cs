using System.Text.Json.Serialization;
using Enums;
using Main.Entities;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Armtek;

public record ArmtekSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierArmtekSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("login")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("password")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingPasswordDescriptionMessage.Key)]
	public string? Password { get; init; }

	[JsonPropertyName("salesOrganization")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingSalesOrganizationNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingSalesOrganizationDescriptionMessage.Key)]
	public string? SalesOrganization { get; init; }

	[JsonPropertyName("customerCode")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingCustomerCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingCustomerCodeDescriptionMessage.Key)]
	public string? CustomerCode { get; init; }

	[JsonPropertyName("contractCode")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingContractCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingContractCodeDescriptionMessage.Key)]
	public string? ContractCode { get; init; }

	[JsonPropertyName("storages")]
	[SchemaFieldLabel(SupplierArmtekSettingStoragesNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingStoragesDescriptionMessage.Key)]
	public IReadOnlyList<ArmtekStorageBindingData>? Storages { get; init; }
}

public record ArmtekSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierArmtekSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel(SupplierArmtekSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("login")]
	[SchemaFieldLabel(SupplierArmtekSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("hasPassword")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierArmtekSettingHasPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingHasPasswordDescriptionMessage.Key)]
	public bool HasPassword { get; init; }

	[JsonPropertyName("salesOrganization")]
	[SchemaFieldLabel(SupplierArmtekSettingSalesOrganizationNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingSalesOrganizationDescriptionMessage.Key)]
	public string? SalesOrganization { get; init; }

	[JsonPropertyName("customerCode")]
	[SchemaFieldLabel(SupplierArmtekSettingCustomerCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingCustomerCodeDescriptionMessage.Key)]
	public string? CustomerCode { get; init; }

	[JsonPropertyName("contractCode")]
	[SchemaFieldLabel(SupplierArmtekSettingContractCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingContractCodeDescriptionMessage.Key)]
	public string? ContractCode { get; init; }

	[JsonPropertyName("storages")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierArmtekSettingStoragesNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingStoragesDescriptionMessage.Key)]
	public IReadOnlyList<ArmtekStorageBindingData> Storages { get; init; } = [];
}

public record ArmtekStorageBindingData
{
	[JsonPropertyName("storageCode")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Storage, "code")]
	[SchemaFieldLabel(SupplierArmtekSettingStorageCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingStorageCodeDescriptionMessage.Key)]
	public string? StorageCode { get; init; }

	[JsonPropertyName("deliveryMode")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.DeliveryMode)]
	[SchemaFieldLabel(SupplierArmtekSettingDeliveryModeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingDeliveryModeDescriptionMessage.Key)]
	[JsonConverter(typeof(JsonStringEnumConverter<DeliveryMode>))]
	public DeliveryMode DeliveryMode { get; init; }

	[JsonPropertyName("pointCode")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierArmtekSettingPointCodeNameMessage.Key)]
	[SchemaFieldDescription(SupplierArmtekSettingPointCodeDescriptionMessage.Key)]
	public string? PointCode { get; init; }
}
