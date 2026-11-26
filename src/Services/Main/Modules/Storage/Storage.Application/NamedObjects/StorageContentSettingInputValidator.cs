using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Storage.Application.NamedObjects;

public class StorageContentSettingInputValidator : AbstractValidator<StorageContentSettingInputData>
{
	public StorageContentSettingInputValidator(INamedObjectRegistry<StorageContentExtractPolicyBase> registry)
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.StorageContentExtractionPolicy)
			.Required()
			.Must(x => registry.TryGetBySystemName(x!) != null)
			.WithFieldError(SettingFieldValueInvalidMessage.Create);
	}
}
