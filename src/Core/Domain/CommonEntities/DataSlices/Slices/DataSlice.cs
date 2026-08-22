using System.Linq.Expressions;
using Domain.CommonEntities.DataSlices.Definitions;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;

namespace Domain.CommonEntities.DataSlices.Slices;

public abstract class DataSlice :
    Entity<DataSlice, int>,
    ILinqEntity<DataSlice, int>
{
    //Not guid cuz it will be to match for slice.
    public int Id { get; private set; }

    public Guid DataSliceDefinitionId { get; private set; }
    public Guid RevisionId { get; private set; }
    public string? Payload { get; private set; }
    public DataSliceDefinition Definition { get; private set; } = null!;

    protected DataSlice() { }

    protected DataSlice(
        DataSliceDefinition definition,
        string? payload = null)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Definition = definition;
        DataSliceDefinitionId = definition.Id;
        RevisionId = definition.PreparingRevisionId
            .EnsureNotNullOrDefault(() =>
                new InvalidOperationException(
                    "Preparing revision must be initialized."));

        Payload = payload.NullIfWhiteSpace();
        Payload?.EnsureValidJson(() =>
            new InvalidOperationException(
                "Invalid JSON value given as a payload."));
    }

    public static Expression<Func<DataSlice, int>> GetKeySelector()
        => x => x.Id;

    public static Expression<Func<DataSlice, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;

    public override int GetId() => Id;
}
