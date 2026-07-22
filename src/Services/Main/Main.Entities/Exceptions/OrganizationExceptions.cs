using Exceptions.Base;

namespace Main.Entities.Exceptions;

public class OrganizationNotFoundException(Guid organizationId)
    : NotFoundException("organization.not.found", new { OrganizationId = organizationId });
