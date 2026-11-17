using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;

namespace Main.Modules.Notifications.Application.Handlers.NotificationPreferences.
	UpsertNotificationPreferences;

public class UpsertNotificationPreferencesDbValidation(IUserChecks userChecks)
	: AbstractDbValidation<UpsertNotificationPreferencesCommand>
{
	public override void Build(IValidationPlan plan, UpsertNotificationPreferencesCommand request)
		=> userChecks.Exists(plan, request.UserId);
}
