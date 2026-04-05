namespace Abstractions.Interfaces.Services;

public interface IUnitOfWorkContext
{
    bool SuppressAutoSave { get; set; }
}