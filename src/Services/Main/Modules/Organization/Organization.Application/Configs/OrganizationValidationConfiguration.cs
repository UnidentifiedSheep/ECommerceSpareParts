using BulkValidation.Core.Configuration;
using Main.Entities.Exceptions;
using Main.Entities.Organization;

namespace Main.Application.Configs;

public static class OrganizationValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(OrganizationKeys.Id)
			.OnMissing(input => new OrganizationNotFoundException(input.Values[0]));

		errors.For(OrganizationMemberKeys.PK)
			.OnMissing(input => new OrganizationMemberNotFoundException(input.Values));

		errors.For(OrganizationMemberKeys.PK)
			.OnAlreadyExists(input => new OrganizationMemberAlreadyExistsException(input.Values));

		errors.For(OrganizationKeys.SystemName)
			.OnAlreadyExists(input => new OrganizationSystemNameAlreadyTakenException(input.Values));
	}
}
