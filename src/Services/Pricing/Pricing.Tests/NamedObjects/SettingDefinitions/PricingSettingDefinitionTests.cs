using System.Text.Json;
using System.Text.Json.Nodes;
using Application.Common.Interfaces.Settings;
using Contracts.Analytics;
using Exceptions;
using FluentAssertions;
using Moq;
using Pricing.Application.NamedObjects.SettingDefinitions;
using Pricing.Entities.Settings;
using Tests.Stubs;

namespace Pricing.Integration.Tests.NamedObjects.SettingDefinitions;

public class PricingSettingDefinitionTests
{
    private readonly Mock<ISettingsService> _settingsService = new();
    private readonly MessageBrokerStub _publisher = new();
    private readonly List<PricingSetting> _savedSettings = [];
    private readonly PricingSettingDefinition _definition;

    public PricingSettingDefinitionTests()
    {
        _settingsService
            .Setup(x => x.SetSetting(
                It.IsAny<PricingSetting>(),
                It.IsAny<CancellationToken>()))
            .Callback<PricingSetting, CancellationToken>((setting, _) =>
                _savedSettings.Add(setting))
            .Returns(Task.CompletedTask);

        _definition = new PricingSettingDefinition(
            _settingsService.Object,
            _publisher);
    }

    [Fact]
    public async Task UpdateSettingAsync_WithValidInput_GeneratesNonEmptyVersion()
    {
        await _definition.UpdateSettingAsync(ValidInputJson(), CancellationToken.None);

        _savedSettings.Should().ContainSingle();
        _savedSettings.Single().Data.Version.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task UpdateSettingAsync_WithSameInputRepeatedly_GeneratesNewVersion()
    {
        var json = ValidInputJson();

        await _definition.UpdateSettingAsync(json, CancellationToken.None);
        await _definition.UpdateSettingAsync(json, CancellationToken.None);

        _savedSettings.Should().HaveCount(2);
        _savedSettings[1].Data.Version.Should().NotBe(_savedSettings[0].Data.Version);
    }

    [Fact]
    public async Task UpdateSettingAsync_WithClientVersion_IgnoresClientVersion()
    {
        var clientVersion = Guid.NewGuid();
        var input = JsonNode.Parse(ValidInputJson())!.AsObject();
        input["version"] = clientVersion;

        await _definition.UpdateSettingAsync(input.ToJsonString(), CancellationToken.None);

        var savedVersion = _savedSettings.Single().Data.Version;
        savedVersion.Should().NotBe(Guid.Empty);
        savedVersion.Should().NotBe(clientVersion);
    }

    [Fact]
    public async Task UpdateSettingAsync_WithValidInput_PublishesMarkupRefreshRequestedEvent()
    {
        await _definition.UpdateSettingAsync(ValidInputJson(), CancellationToken.None);

        _publisher
            .PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task UpdateSettingAsync_WithInvalidInput_DoesNotSaveOrPublishEvent()
    {
        var invalidInput = ValidInput() with { DefaultMarkup = -0.1m };

        var action = () => _definition.UpdateSettingAsync(
            JsonSerializer.Serialize(invalidInput),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidInputException>();
        _savedSettings.Should().BeEmpty();
        _publisher
            .PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>()
            .Should()
            .BeEmpty();
    }

    private static string ValidInputJson() => JsonSerializer.Serialize(ValidInput());

    private static PricingSettingInputData ValidInput() => new()
    {
        SelectedMarkupId = 1,
        DefaultMarkup = 0.2m,
        OfferTtl = TimeSpan.FromDays(1),
        PriceRoundingStep = 0.01m,
        DeliveryDayPenalty = 2m,
        UniqProductAdditionalMarkup = 0.2m
    };
}
