using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Document;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.Document;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using NamedObject.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;

namespace Main.Application.Lrts.GenerateDocument;

public class GenerateDocumentLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	INamedObjectRegistry<IDocumentDefinition> registry,
	INotificationService notificationService,
	IAppLinkProvider appLinkProvider,
	IContextualLocalizer localizer,
	ILocalizer notificationLocalizer,
	IJsonSerializer jsonSerializer,
	ILogger<GenerateDocumentLrt> logger) : LrtBase<GenerateDocumentInputState, GenerateDocumentState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	private static readonly TimeSpan DocumentLifetime = TimeSpan.FromDays(30);
	public static string Name => nameof(GenerateDocumentLrt);
	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtDocumentGenerationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtDocumentGenerationDescriptionMessage.Instance;

	protected override async Task DoWork(GenerateDocumentInputState inputState)
	{
		var definition = string.IsNullOrWhiteSpace(inputState.DocumentSystemName)
			? null
			: registry.TryGetBySystemName(inputState.DocumentSystemName);

		if (definition == null)
		{
			Interrupt(localizer.Get(
				LrtDocumentGenerationUnknownSystemNameMessage.Instance));
			return;
		}

		if (!TryDeserializeDocumentRequest(inputState, definition, out var request) ||
		    !DocumentCulture.TryGetSupported(request.Culture, out var culture))
		{
			Interrupt(localizer.Get(
				LrtDocumentGenerationInvalidRequestMessage.Instance));
			return;
		}

		await GenerateIfNeededAsync(definition, request);
		if (!State.NotificationProcessed)
			await CompleteRequestAsync(definition, culture);
	}

	private bool TryDeserializeDocumentRequest(
		GenerateDocumentInputState inputState,
		IDocumentDefinition definition,
		[NotNullWhen(true)] out IDocumentRequest? request)
	{
		request = null;
		if (string.IsNullOrWhiteSpace(inputState.DocumentRequest) ||
		    !jsonSerializer.TryDeserialize(
				inputState.DocumentRequest,
				definition.RequestType,
				out var deserializedRequest))
			return false;

		request = deserializedRequest as IDocumentRequest;
		return request is not null;
	}

	private async Task GenerateIfNeededAsync(IDocumentDefinition definition, IDocumentRequest request)
	{
		if (State is
		    {
			    BucketName: not null,
			    StorageKey: not null,
			    GeneratedAtUtc: not null
		    })
			return;

		var result = await definition.GenerateAsync(request, CancellationToken);

		if (string.IsNullOrWhiteSpace(result.BucketName) ||
		    string.IsNullOrWhiteSpace(result.StorageKey))
			throw new InvalidOperationException("Generated document response is missing storage details.");

		await SaveStateAsync(new GenerateDocumentState
		{
			BucketName = result.BucketName,
			StorageKey = result.StorageKey,
			GeneratedAtUtc = DateTime.UtcNow
		});
	}

	private async Task CompleteRequestAsync(IDocumentDefinition definition, CultureInfo culture)
	{
		var bucketName = State.BucketName;
		var storageKey = State.StorageKey;
		var generatedAtUtc = State.GeneratedAtUtc;

		if (string.IsNullOrWhiteSpace(bucketName) ||
		    string.IsNullOrWhiteSpace(storageKey) ||
		    generatedAtUtc is not { Kind: DateTimeKind.Utc })
			throw new InvalidOperationException("Generated document storage details are missing.");

		await TransactionService.ExecuteAsync(
			settings: TransactionalAttribute.ReadCommitted(30, 3),
			action: async (ctx, ct) =>
			{
				var request = await ctx.Repositories.Get<DocumentGenerationRequest, Guid>()
					.FirstOrDefaultAsync(
						Criteria<DocumentGenerationRequest>.New()
							.Track()
							.Where(x => x.JobId == Job.Id)
							.Build(),
						ct);

				if (request is not null)
				{
					if (request.StorageKey is null)
						request.Complete(
							bucketName,
							storageKey,
							generatedAtUtc.Value,
							generatedAtUtc.Value.Add(DocumentLifetime));
					else if (request.BucketName != bucketName ||
					         request.StorageKey != storageKey)
						throw new InvalidOperationException(
							"Document generation request is already completed with different storage details.");

					await QueueNotificationAsync(request, definition, culture, ct);
				}

				await SaveStateAsync(State with { NotificationProcessed = true });
			},
			cancellationToken: CancellationToken);
	}

	private async Task QueueNotificationAsync(
		DocumentGenerationRequest request,
		IDocumentDefinition definition,
		CultureInfo culture,
		CancellationToken token)
	{
		if (request.RequesterId == null) return;

		var documentUrl = await appLinkProvider
			.CreateDocumentUrlAsync(request.RequestId, token);

		await notificationService.QueueAsync(
			request.RequesterId.Value,
			new DocumentGeneratedNotification(
				new DocumentGeneratedNotificationData(
					notificationLocalizer,
					culture,
					definition.Name,
					definition.Description,
					request.RequestId,
					documentUrl.AbsoluteUri)),
			[new InAppRecipient(request.RequesterId.Value)],
			token);
	}
}
