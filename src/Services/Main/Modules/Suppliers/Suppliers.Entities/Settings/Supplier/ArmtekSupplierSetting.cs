using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Suppliers.Settings.Supplier;

public class ArmtekSupplierSetting : Setting<ArmtekSupplierSettingData>, ISetting<ArmtekSupplierSetting>
{
	public ArmtekSupplierSetting(string json) : base(SettingName, json)
	{
	}

	public ArmtekSupplierSetting(ArmtekSupplierSettingData data) : base(SettingName, data)
	{
	}

	public static string SettingName => "ArmtekSupplierSetting";

	public static ArmtekSupplierSetting Default => new(new ArmtekSupplierSettingData());
}

public record ArmtekSupplierSettingData
{
	public const int SalesOrganizationMaxLength = 4;

	public const int CustomerCodeMaxLength = 10;

	public const int ContractCodeMaxLength = 10;

	[JsonPropertyName("isEnabled")]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("login")]
	public string? Login { get; init; }

	[JsonPropertyName("encryptedPassword")]
	public string? EncryptedPassword { get; init; }

	[JsonPropertyName("salesOrganization")]
	public string? SalesOrganization { get; init; }

	[JsonPropertyName("customerCode")]
	public string? CustomerCode { get; init; }

	[JsonPropertyName("contractCode")]
	public string? ContractCode { get; init; }

	[JsonPropertyName("storages")]
	public IReadOnlyList<ArmtekStorageBinding> Storages { get; init; } = [];
}

public record ArmtekStorageBinding
{
	public const int PointCodeMaxLength = 10;

	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }

	[JsonPropertyName("deliveryMode")]
	[JsonConverter(typeof(JsonStringEnumConverter<DeliveryMode>))]
	public required DeliveryMode DeliveryMode { get; init; }

	[JsonPropertyName("pointCode")]
	public required string PointCode { get; init; }
}
