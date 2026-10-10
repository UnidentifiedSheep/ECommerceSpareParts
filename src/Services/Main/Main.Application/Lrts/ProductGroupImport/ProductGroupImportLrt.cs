using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Exceptions.Interfaces;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductGroupImport;

public class ProductGroupImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductGroupImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductGroupImportInputState, ProductGroupImportState,
		ProductGroupImportLrt.NewProductGroupCsvDto, ProductGroup>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public override string SystemName => nameof(ProductGroupImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductGroupImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductGroupImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ProductGroupImportTooManyErrorsMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		NewProductGroupCsvDto row,
		ProductGroupImportState state,
		List<CsvImportError> errors,
		out ProductGroup item)
	{
		item = null!;
		try
		{
			item = ProductGroup.Create(row.Name);
			return true;
		}
		catch (Exception ex)
		{
			var message = ex is ILocalizableException localizableException
				? StringLocalizer.Get(localizableException.LocalizableMessage)
				: ex.Message;
			errors.Add(CreateError(rowIdx, message));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductGroup item)> groups,
		ProductGroupImportState state,
		List<CsvImportError> errors)
	{
		if (groups.Count == 0)
			return;

		var firstIdx = groups[0].idx;
		var uniqueNames = new HashSet<string>();
		var uniqueGroups = new List<(int idx, ProductGroup item)>();
		foreach (var group in groups)
		{
			if (uniqueNames.Add(group.item.NormalizedName))
				uniqueGroups.Add(group);
			else
				state.SkippedLines.Add(group.idx);
		}

		var result = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingNames = (await context.Repositories
					.GetForRead<ProductGroup, int>()
					.Query
					.Where(group => uniqueNames.Contains(group.NormalizedName))
					.Select(group => group.NormalizedName)
					.ToListAsync(cancellationToken))
					.ToHashSet();

				var toCreate = uniqueGroups
					.Where(group => !existingNames.Contains(group.item.NormalizedName))
					.ToList();

				await context.UnitOfWork.AddRangeAsync(
					toCreate.Select(group => group.item).ToList(),
					cancellationToken);

				await context.UnitOfWork.SaveChangesAsync(cancellationToken);

				return new ProductGroupImportBatchResult(
					toCreate.Count,
					uniqueGroups
						.Where(group => existingNames.Contains(group.item.NormalizedName))
						.Select(group => group.idx)
						.ToList());
			},
			CancellationToken);
		state.SkippedLines.AddRange(result.SkippedLines);

		Logger.LogInformation(
			"Product group import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}",
			JobId,
			firstIdx,
			groups.Count,
			result.Created,
			groups.Count - result.Created);
	}

	private sealed record ProductGroupImportBatchResult(int Created, IReadOnlyList<int> SkippedLines);

	public record NewProductGroupCsvDto
	{
		[Name("Name")]
		public required string Name { get; init; }
	}
}
