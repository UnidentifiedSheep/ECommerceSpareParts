using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Platform.Settings;

namespace Main.Modules.Platform.Application.NamedObjects;

public class GlobalApplicationSettingInputValidator : AbstractValidator<GlobalApplicationSettingData>
{
	public GlobalApplicationSettingInputValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.ApiServiceUrl).Required().HttpUrl();

		RuleFor(x => x.AppServiceUrl).Required().HttpUrl();
	}
}
