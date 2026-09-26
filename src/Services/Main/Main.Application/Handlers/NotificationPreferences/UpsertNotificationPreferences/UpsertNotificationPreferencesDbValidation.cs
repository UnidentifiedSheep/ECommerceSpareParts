using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.NotificationPreferences.UpsertNotificationPreferences;

public class UpsertNotificationPreferencesDbValidation : AbstractDbValidation<UpsertNotificationPreferencesCommand>
{
	public override void Build(IValidationPlan plan, UpsertNotificationPreferencesCommand request)
		=> plan.ValidateUserExistsId(request.UserId);
}
