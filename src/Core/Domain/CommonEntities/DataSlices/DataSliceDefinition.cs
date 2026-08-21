using System.Linq.Expressions;
using Domain.Interfaces;
using Domain.Validation;

namespace Domain.CommonEntities.DataSlices;

public abstract class DataSliceDefinition :
    Entity<DataSliceDefinition, Guid>,
    ILinqEntity<DataSliceDefinition, Guid>,
    IVersionable<uint>
{
    public Guid Id { get; private set; }
    public Guid DataSliceGroupId { get; private set; }

    public Guid? PublishedRevisionId { get; private set; }
    public Guid? PreparingRevisionId { get; private set; }
    public bool IsDirty { get; private set; }
    public uint RowVersion { get; private set; }

    public DataSliceGroup Group { get; private set; } = null!;

    private readonly List<DataSlice> _slices = [];
    public IReadOnlyList<DataSlice> Slices => _slices;

    protected DataSliceDefinition() { }

    protected DataSliceDefinition(DataSliceGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        Group = group;
        DataSliceGroupId = group.Id;
        IsDirty = true;
    }

    public void BeginRevision(Guid revisionId)
    {
        PreparingRevisionId
            .EnsureNullOrDefault(
                () => new InvalidOperationException(
                    "A data slice revision is already being prepared."));

        PreparingRevisionId = revisionId.EnsureNotEqual(
            Guid.Empty,
            () => new InvalidOperationException(
                "Preparing revision id cannot be empty."));

        // Changes received after this point will mark the definition dirty again.
        IsDirty = false;
    }

    public void PublishRevision(Guid revisionId)
    {
        EnsureOwnsPreparingRevision(revisionId);

        PublishedRevisionId = revisionId;
        ClearPreparingRevision();
    }

    public void AbortRevision(Guid revisionId)
    {
        EnsureOwnsPreparingRevision(revisionId);

        ClearPreparingRevision();
        IsDirty = true;
    }

    public void MarkDirty() => IsDirty = true;

    public bool IsPreparing => PreparingRevisionId.HasValue;
    public bool HasPublishedRevision => PublishedRevisionId.HasValue;

    public bool RequiresRecalculation =>
        IsDirty ||
        PublishedRevisionId is null && PreparingRevisionId is null;

    public static Expression<Func<DataSliceDefinition, Guid>> GetKeySelector()
        => definition => definition.Id;

    public static Expression<Func<DataSliceDefinition, bool>> GetEqualityExpression(Guid key)
        => definition => definition.Id == key;

    public override Guid GetId() => Id;

    private void EnsureOwnsPreparingRevision(Guid revisionId)
    {
        revisionId.EnsureNotEqual(
            Guid.Empty,
            () => new InvalidOperationException(
                "Revision id cannot be empty."));

        if (PreparingRevisionId != revisionId)
            throw new InvalidOperationException(
                "The definition does not own the specified preparing revision.");
    }

    private void ClearPreparingRevision()
    {
        PreparingRevisionId = null;
    }
}
