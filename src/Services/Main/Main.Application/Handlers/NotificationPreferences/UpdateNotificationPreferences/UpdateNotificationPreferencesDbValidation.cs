using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.NotificationPreferences.UpdateNotificationPreferences;

public class UpdateNotificationPreferencesDbValidation : AbstractDbValidation<UpdateNotificationPreferencesCommand>
{
	public override void Build(IValidationPlan plan, UpdateNotificationPreferencesCommand request)
		=> plan.ValidateUserExistsId(request.UserId);
}
