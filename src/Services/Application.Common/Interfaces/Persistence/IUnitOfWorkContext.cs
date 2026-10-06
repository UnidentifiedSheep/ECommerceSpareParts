namespace Application.Common.Interfaces.Persistence;

public interface IUnitOfWorkContext
{
	bool SuppressAutoSave { get; set; }
}
