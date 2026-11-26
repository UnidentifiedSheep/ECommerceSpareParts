using System.Text.Json;
using Application.Common.Interfaces.Settings;
using Domain.CommonEntities;
using Domain.Interfaces;
using Exceptions;
using FluentValidation;
using Main.Entities;

namespace Application.Common.NamedObject;

public abstract class SettingDefinitionBase<TSetting, TData, TInput, TOutput>(
	ISettingsService settingsService,
	IValidator<TInput> inputValidator,
	IValidator<TData>? dataValidator = null) : SettingDefinitionNamedObjectBase<TSetting>(settingsService)
	where TSetting : Setting<TData>, ISetting<TSetting>
	where TInput : class
{
	public override Type InputSettingType => typeof(TInput);

	public override Type OutputSettingType => typeof(TOutput);

	public sealed override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var input = Deserialize(json);
		await inputValidator.ValidateAndThrowAsync(input, cancellationToken);

		var current = await SettingsService.GetOrDefault<TSetting>(cancellationToken);
		var data = Map(input, current.Data);
		if (dataValidator is not null)
			await dataValidator.ValidateAndThrowAsync(data, cancellationToken);

		await SettingsService.SetSetting(CreateSetting(data), cancellationToken);
		await OnUpdatedAsync(data, cancellationToken);
	}

	public sealed override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
	{
		var setting = await SettingsService.GetOrDefault<TSetting>(cancellationToken);
		return JsonSerializer.Serialize(ToOutput(setting.Data));
	}

	protected abstract TData Map(TInput input, TData current);

	protected abstract TOutput ToOutput(TData data);

	protected abstract TSetting CreateSetting(TData data);

	protected virtual Task OnUpdatedAsync(TData data, CancellationToken cancellationToken) => Task.CompletedTask;

	private static TInput Deserialize(string json)
	{
		try
		{
			return JsonSerializer.Deserialize<TInput>(json) ??
				throw new InvalidInputException(SettingInputInvalidMessage.Instance);
		}
		catch (JsonException)
		{
			throw new InvalidInputException(SettingInputInvalidMessage.Instance);
		}
	}
}
