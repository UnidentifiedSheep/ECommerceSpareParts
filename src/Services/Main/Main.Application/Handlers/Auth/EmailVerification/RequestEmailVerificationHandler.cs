using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Exceptions;
using Localization.Abstractions.Interfaces;
using Mailing.Core.Models;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Enums.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.EmailVerification;

[Transactional]
[AutoSave]
public record RequestEmailVerificationCommand(Guid UserId, string Email) : ICommand;

public class RequestEmailVerificationHandler(
	IRepository<UserEmail, string> repository,
	IJsonSigner jsonSigner,
	IMailingService mailingService,
	IVerificationPayloadProvider verificationPayloadProvider,
	IContextualStringLocalizer localizer,
	ISettingsService settingsService) : ICommandHandler<RequestEmailVerificationCommand>
{
	public async Task<Unit> Handle(
		RequestEmailVerificationCommand request,
		CancellationToken cancellationToken)
	{
		var normalizedEmail = Email.ToNormalized(request.Email);
		var criteria = Criteria<UserEmail>.New().Where(x => x.Email == normalizedEmail).Track(false).Build();
		var userMail = await repository.FirstOrDefaultAsync(criteria, cancellationToken);

		if (userMail == null || userMail.UserId != request.UserId)
			throw new UserEmailNotFoundException(normalizedEmail);

		if (userMail.Confirmed)
			return Unit.Value;

		var setting = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Data;
		var appServiceUrl = setting.AppServiceUrl ??
			throw new InvalidInputException("global.application.setting.app.service.url.not.configured");

		var signed = jsonSigner.Sign(
			await verificationPayloadProvider.GetPayload(
				request.UserId,
				VerificationType.EmailVerification,
				normalizedEmail));

		var baseUri = new Uri(appServiceUrl.TrimEnd('/') + "/");
		var verificationUrl = new Uri(baseUri, $"verify-email?token={Uri.EscapeDataString(signed)}");

		await mailingService.QueueEmailAsync(
			new EmailVerificationData(
				localizer,
				verificationUrl.ToString(),
				normalizedEmail),
			cancellationToken);

		return Unit.Value;
	}
}
