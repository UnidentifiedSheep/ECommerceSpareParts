using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
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
		ProductGroupImportLrt.ProductGroupCsvDto, ProductGroup>(
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
		ProductGroupCsvDto row,
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
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
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
		var uniqueGroups = groups
			.Select(group => group.item)
			.DistinctBy(group => group.NormalizedName)
			.ToList();
		var uniqueNames = uniqueGroups.Select(group => group.NormalizedName).ToArray();

		var created = await TransactionService.ExecuteAsync(
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
					.Where(group => !existingNames.Contains(group.NormalizedName))
					.ToList();

				await context.UnitOfWork.AddRangeAsync(toCreate, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);

				return toCreate.Count;
			},
			CancellationToken);

		Logger.LogInformation(
			"Product group import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}",
			JobId,
			firstIdx,
			groups.Count,
			created,
			groups.Count - created);
	}

	public record ProductGroupCsvDto
	{
		[Name("Name")]
		public required string Name { get; init; }
	}
}
