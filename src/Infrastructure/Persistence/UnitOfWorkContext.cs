using Application.Common.Interfaces.Persistence;

namespace Persistence;

public class UnitOfWorkContext : IUnitOfWorkContext
{
	public bool SuppressAutoSave { get; set; }
}
