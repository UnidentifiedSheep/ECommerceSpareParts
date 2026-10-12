using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductImport;

public sealed class ProductImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<ProductImportLrt> logger)
	: MultiStepLrtBase<ProductImportInputState, ProductImportInputState>(
		jobRepository, unitOfWork, publisher, transactionService, logger)
{
	public override string SystemName => nameof(ProductImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductImportDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder, string initialState)
	{
		var import = builder.AddStep(nameof(ProductImportLrtStep), initialState);
		var synchronization = builder.AddStep(nameof(ProductSynchronizationLrt), NoneInputState.Json);
		builder.AddDependency(synchronization, import);
	}
}
