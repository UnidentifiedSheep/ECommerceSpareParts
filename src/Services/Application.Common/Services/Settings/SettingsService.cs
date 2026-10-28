using Application.Common.Extensions;
using System.Data;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.Services.Settings;

public class SettingsService(
	IRepository<Setting, string> repository,
	IApplicationTransaction transactionService,
	ISettingsContainer settingsContainer) : ISettingsService
{
	private static readonly TransactionalAttribute TransactionSettings = new(
		IsolationLevel.ReadCommitted,
		20,
		3);

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		var criteria = Criteria<Setting>.New().Track(false).Build();

		var dbSettings = await repository.ListAsync(criteria, cancellationToken);
		settingsContainer.Load(dbSettings);
	}

	public async Task SetSetting<T>(T value, CancellationToken cancellationToken = default) where T : Setting
	{
		await transactionService.WithSettingsIfIdle(TransactionSettings).ExecuteAsync(
			async (context, ct) =>
			{
				var criteria = Criteria<Setting>
					.New()
					.Where(x => x.Key == value.Key)
					.ForUpdate()
					.Track()
					.Build();

				var existing = await repository.FirstOrDefaultAsync(criteria, ct);
				existing?.SetData(value.Json);

				if (existing == null)
					await context.UnitOfWork.AddAsync(value, ct);

				await context.UnitOfWork.SaveChangesAsync(ct);
			},
			cancellationToken);

		settingsContainer.Set(value);
	}

	public Task<T> GetOrDefault<T>(CancellationToken cancellationToken = default)
		where T : Setting, ISetting<T> => GetOrDefault<T>(false, cancellationToken);

	public async Task<T> GetOrDefault<T>(bool directRead, CancellationToken cancellationToken = default)
		where T : Setting, ISetting<T>
	{
		if (!directRead && settingsContainer.TryGet<T>(out var setting))
			return setting!;

		var settingName = T.SettingName;
		var dbSetting = directRead
			? await repository.FirstOrDefaultAsync(Criteria<Setting>.New()
				.Where(x => x.Key == settingName).Track(false).Build(), cancellationToken)
			: await repository.GetById(settingName, cancellationToken);

		if (dbSetting != null)
		{
			var typed = dbSetting as T ?? throw new InvalidOperationException(
				$"Setting '{dbSetting.Key}' was materialized as " +
				$"'{dbSetting.GetType().Name}' instead of '{typeof(T).Name}'.");
			if (!directRead)
				settingsContainer.Set(typed);
			return typed;
		}

		var defaultSetting = T.Default;
		if (!directRead)
			await SetSetting(defaultSetting, cancellationToken);
		return defaultSetting;
	}
}
