using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class OrganizationNotFoundException : LocalizedNotFoundException
{
	public OrganizationNotFoundException(Guid organizationId) : base(
		OrganizationNotFoundMessage.Instance,
		new
		{
			OrganizationId = organizationId
		})
	{
	}

	public OrganizationNotFoundException(string systemName) : base(
		OrganizationNotFoundMessage.Instance,
		new
		{
			SystemName = systemName
		})
	{
	}
}
