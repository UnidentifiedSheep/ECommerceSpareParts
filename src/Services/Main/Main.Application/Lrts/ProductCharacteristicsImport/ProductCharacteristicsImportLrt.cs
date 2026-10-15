using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities;
using Main.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.ProductCharacteristicsImport;

public sealed class ProductCharacteristicsImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger<ProductCharacteristicsImportLrt> logger)
	: MultiStepLrtBase<ProductCharacteristicsImportInputState, ProductCharacteristicsImportInputState>(
		jobRepository, unitOfWork, publisher, transactionService, logger)
{
	public const string Name = nameof(ProductCharacteristicsImportLrt);

	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductCharacteristicsImportNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductCharacteristicsImportDescriptionMessage.Instance;

	protected override void ConfigureSteps(IMultiStepJobBuilder builder, string initialState)
	{
		var import = builder.AddStep(ProductCharacteristicsImportLrtStep.Name, initialState);
		var synchronization = builder.AddStep(
			ProductSynchronizationLrt.Name,
			JsonSerializer.Serialize(
				new ProductSynchronizationInputState
				{
					Parts = ProductEventParts.Characteristics
				}));
		builder.AddDependency(synchronization, import);
	}
}
