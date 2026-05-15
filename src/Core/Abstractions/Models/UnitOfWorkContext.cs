namespace Abstractions.Models;

public class UnitOfWorkContext
{
    public bool SuppressAutoSave { get; set; }
    public bool UseSystemContextWhenUserContextNull { get; set; }
}