using Abstractions.Interfaces.Services;

namespace Abstractions.Models;

public class UnitOfWorkContext : IUnitOfWorkContext
{
    // ReSharper disable once InconsistentNaming
    private static readonly AsyncLocal<bool> _suppressAutoSave = new();

    public bool SuppressAutoSave
    {
        get => _suppressAutoSave.Value;
        set => _suppressAutoSave.Value = value;
    }
}