using Exceptions.Base;

namespace Main.Entities.Exceptions;

public class OrganizationNotFoundException : NotFoundException
{
	public OrganizationNotFoundException(Guid organizationId) : base(
		"organization.not.found",
		new
		{
			OrganizationId = organizationId
		})
	{
	}

	public OrganizationNotFoundException(string systemName) : base(
		"organization.not.found",
		new
		{
			SystemName = systemName
		})
	{
	}
}
