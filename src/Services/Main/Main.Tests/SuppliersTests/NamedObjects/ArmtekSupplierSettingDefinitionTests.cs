using System.Text.Json;
using System.Text.Json.Nodes;
using Application.Common.Interfaces.Settings;
using Enums;
using Exceptions;
using FluentAssertions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Suppliers.Settings.Supplier;
using Main.Modules.Suppliers.Application.NamedObjects.Supplier.Armtek;
using Moq;
using Security.Core.Interfaces;

namespace Tests.SuppliersTests.NamedObjects;

public class ArmtekSupplierSettingDefinitionTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private readonly ArmtekSupplierSettingDefinition _definition;

	private readonly List<ArmtekSupplierSetting> _savedSettings = [];

	private readonly Mock<ISettingsService> _settingsService = new();

	private ArmtekSupplierSettingData _current = new();

	public ArmtekSupplierSettingDefinitionTests()
	{
		_settingsService
			.Setup(x => x.GetOrDefault<ArmtekSupplierSetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new ArmtekSupplierSetting(_current));
		_settingsService
			.Setup(x => x.SetSetting(It.IsAny<ArmtekSupplierSetting>(), It.IsAny<CancellationToken>()))
			.Callback<ArmtekSupplierSetting, CancellationToken>((setting, _) => _savedSettings.Add(setting))
			.Returns(Task.CompletedTask);

		var encryptor = new Mock<ISecretEncryptor>();
		encryptor.Setup(x => x.Encrypt(It.IsAny<string>())).Returns<string>(x => $"enc:{x}");

		_definition = new ArmtekSupplierSettingDefinition(
			_settingsService.Object,
			encryptor.Object,
			new ArmtekSupplierSettingInputValidator(),
			new ArmtekSupplierSettingDataValidator());
	}

	[Fact]
	public async Task UpdateSettingAsync_WithValidInput_SavesNormalizedData()
	{
		await _definition.UpdateSettingAsync(ValidInput().ToJsonString(), CancellationToken);

		var data = _savedSettings.Single().Data;
		data.IsEnabled.Should().BeTrue();
		data.BaseUrl.Should().Be("https://ws.armtek.ru");
		data.Login.Should().Be("user");
		data.EncryptedPassword.Should().Be("enc:secret");
		data.SalesOrganization.Should().Be("4000");
		data.CustomerCode.Should().Be("0000123456");
		data.ContractCode.Should().BeNull();
		data.Storages.Should().BeEquivalentTo(
		[
			new ArmtekStorageBinding
			{
				StorageCode = "MAIN-01", DeliveryMode = DeliveryMode.Pickup, PointCode = "0000001234"
			}
		]);
	}

	[Fact]
	public async Task UpdateSettingAsync_WithNullFields_KeepsCurrentValues()
	{
		_current = CurrentData();

		await _definition.UpdateSettingAsync("""{ "isEnabled": true }""", CancellationToken);

		_savedSettings.Single().Data.Should().BeEquivalentTo(_current);
	}

	[Fact]
	public async Task UpdateSettingAsync_WithEmptyContractCode_ClearsContractCode()
	{
		_current = CurrentData() with
		{
			ContractCode = "0000000001"
		};

		await _definition.UpdateSettingAsync(
			"""{ "isEnabled": true, "contractCode": " " }""",
			CancellationToken);

		_savedSettings.Single().Data.ContractCode.Should().BeNull();
	}

	[Fact]
	public async Task UpdateSettingAsync_WhenDisabled_SavesIncompleteData()
	{
		await _definition.UpdateSettingAsync(
			"""{ "isEnabled": false, "login": "user" }""",
			CancellationToken);

		var data = _savedSettings.Single().Data;
		data.IsEnabled.Should().BeFalse();
		data.Login.Should().Be("user");
		data.EncryptedPassword.Should().BeNull();
	}

	[Fact]
	public async Task UpdateSettingAsync_WhenEnabledWithoutRequiredValues_ThrowsRequiredErrors()
	{
		var errors = await GetErrors("""{ "isEnabled": true, "login": "user" }""");

		errors.Select(x => x.PropertyName)
			.Should()
			.BeEquivalentTo("BaseUrl", "Password", "SalesOrganization", "CustomerCode");
		errors.Single(x => x.PropertyName == "Password")
			.CustomState.Should()
			.BeEquivalentTo(SettingFieldRequiredMessage.Create("password"));
		_savedSettings.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateSettingAsync_WithInvalidFields_ThrowsFieldErrors()
	{
		var input = ValidInput();
		input["baseUrl"] = "ftp://ws.armtek.ru";
		input["password"] = " ";
		input["salesOrganization"] = "40000";

		var errors = await GetErrors(input.ToJsonString());

		errors.Select(x => x.CustomState)
			.Should()
			.BeEquivalentTo(
				new object[]
				{
					SettingFieldUrlInvalidMessage.Create("baseUrl"),
					SettingFieldEmptyMessage.Create("password"),
					SettingFieldMaxLengthMessage.Create("salesOrganization", 4)
				});
	}

	[Fact]
	public async Task UpdateSettingAsync_WithInvalidStorage_ThrowsErrorWithItemPath()
	{
		var input = ValidInput();
		input["storages"] = new JsonArray(
			Storage("MAIN-01", "Pickup", "0000001234"),
			Storage("MAIN-02", "Delivery", ""));

		var errors = await GetErrors(input.ToJsonString());

		errors.Should().ContainSingle();
		errors.Single().PropertyName.Should().Be("Storages[1].PointCode");
		errors.Single().CustomState.Should().BeEquivalentTo(SettingFieldRequiredMessage.Create("storages[1].pointCode"));
	}

	[Fact]
	public async Task UpdateSettingAsync_WithDuplicateStorages_ThrowsDuplicateError()
	{
		var input = ValidInput();
		input["storages"] = new JsonArray(
			Storage("MAIN-01", "Pickup", "0000001234"),
			Storage(" main-01 ", "Delivery", "0000005678"));

		var errors = await GetErrors(input.ToJsonString());

		errors.Single().CustomState.Should().BeEquivalentTo(SettingFieldDuplicateMessage.Create("storages"));
	}

	[Fact]
	public async Task UpdateSettingAsync_WithMalformedJson_ThrowsInvalidInput()
	{
		var action = () => _definition.UpdateSettingAsync("{", CancellationToken);

		await action.Should().ThrowAsync<InvalidInputException>();
	}

	[Fact]
	public async Task GetOutputJsonAsync_HidesPassword()
	{
		_current = CurrentData();

		var json = JsonNode.Parse(await _definition.GetOutputJsonAsync(CancellationToken))!.AsObject();

		json["hasPassword"]!.GetValue<bool>().Should().BeTrue();
		json.ContainsKey("password").Should().BeFalse();
		json.ContainsKey("encryptedPassword").Should().BeFalse();
		json["storages"]![0]!["deliveryMode"]!.GetValue<string>().Should().Be("Delivery");
	}

	private async Task<IReadOnlyList<FluentValidation.Results.ValidationFailure>> GetErrors(string json)
	{
		var action = () => _definition.UpdateSettingAsync(json, CancellationToken);
		var exception = await action.Should().ThrowAsync<ValidationException>();
		return exception.Which.Errors.ToList();
	}

	private static JsonObject ValidInput()
		=> new()
		{
			["isEnabled"] = true,
			["baseUrl"] = " https://ws.armtek.ru/ ",
			["login"] = " user ",
			["password"] = "secret",
			["salesOrganization"] = "4000",
			["customerCode"] = "0000123456",
			["storages"] = new JsonArray(Storage(" MAIN-01 ", "Pickup", " 0000001234 "))
		};

	private static JsonObject Storage(string storageCode, string deliveryMode, string pointCode)
		=> new()
		{
			["storageCode"] = storageCode,
			["deliveryMode"] = deliveryMode,
			["pointCode"] = pointCode
		};

	private static ArmtekSupplierSettingData CurrentData()
		=> new()
		{
			IsEnabled = true,
			BaseUrl = "https://ws.armtek.ru",
			Login = "user",
			EncryptedPassword = "enc:old",
			SalesOrganization = "4000",
			CustomerCode = "0000123456",
			ContractCode = null,
			Storages =
			[
				new ArmtekStorageBinding
				{
					StorageCode = "MAIN-02",
					DeliveryMode = DeliveryMode.Delivery,
					PointCode = "0000005678"
				}
			]
		};
}
