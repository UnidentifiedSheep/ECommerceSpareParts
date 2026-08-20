using System.Linq.Expressions;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;

namespace Domain.CommonEntities.DataSlices;

public abstract class DataSliceGroup :
    AuditableEntity<DataSliceGroup, Guid>,
    ILinqEntity<DataSliceGroup, Guid>,
    IVersionable<uint>
{
    public const int SystemNameMaxLength = 128;

    public Guid Id { get; private set; }
    public string SystemName { get; private set; } = null!;
    public uint RowVersion { get; private set; }

    protected DataSliceGroup() { }

    protected DataSliceGroup(string systemName)
    {
        SystemName = systemName
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace(() =>
                new InvalidOperationException(
                    "Data slice group system name cannot be empty."))
            .EnsureMaxLength(
                SystemNameMaxLength,
                () => new InvalidOperationException(
                    $"Data slice group system name cannot exceed {SystemNameMaxLength} characters."));
    }

    public static Expression<Func<DataSliceGroup, Guid>> GetKeySelector()
        => g => g.Id;

    public static Expression<Func<DataSliceGroup, bool>> GetEqualityExpression(Guid key)
        => g => g.Id == key;

    public override Guid GetId() => Id;
}
