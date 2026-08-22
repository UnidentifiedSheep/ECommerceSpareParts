using Domain.CommonEntities.DataSlices;
using Domain.CommonEntities.DataSlices.Definitions;
using Domain.CommonEntities.DataSlices.Slices;
using Domain.Interfaces;
using FluentAssertions;

namespace Tests.Tests.CommonEntities;

public sealed class DataSliceTests
{
    [Fact]
    public void NewGroup_InitializesSystemName()
    {
        var group = new TestDataSliceGroup("  test.group  ");

        group.SystemName.Should().Be("test.group");
        group.Should().BeAssignableTo<IVersionable<uint>>();
    }

    [Fact]
    public void NewDefinition_RequiresInitialCalculation()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);

        definition.IsDirty.Should().BeTrue();
        definition.PublishedRevisionId.Should().BeNull();
        definition.PreparingRevisionId.Should().BeNull();
        definition.RequiresRecalculation.Should().BeTrue();
    }

    [Fact]
    public void BeginRevision_PreparesRequestedRevisionAndConsumesDirtyState()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        var revisionId = Guid.NewGuid();

        definition.BeginRevision(revisionId);

        definition.PreparingRevisionId.Should().Be(revisionId);
        definition.IsPreparing.Should().BeTrue();
        definition.IsDirty.Should().BeFalse();
        definition.RequiresRecalculation.Should().BeFalse();
    }

    [Fact]
    public void Slice_CapturesPreparingRevision()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        var revisionId = Guid.NewGuid();
        definition.BeginRevision(revisionId);

        var slice = new TestDataSlice(definition, "{\"value\":42}");

        slice.RevisionId.Should().Be(revisionId);
        slice.Definition.Should().BeSameAs(definition);
        slice.Payload.Should().Be("{\"value\":42}");
    }

    [Fact]
    public void Slice_WithoutPayload_StoresOnlyTypedFields()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        definition.BeginRevision(Guid.NewGuid());

        var slice = new TestDataSlice(definition);

        slice.Payload.Should().BeNull();
    }

    [Fact]
    public void Slice_WithInvalidPayload_Throws()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        definition.BeginRevision(Guid.NewGuid());

        var action = () => new TestDataSlice(definition, "not-json");

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Invalid JSON value given as a payload.");
    }

    [Fact]
    public void PublishRevision_PublishesPreparedSchemaAndClearsPreparation()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        var revisionId = Guid.NewGuid();
        definition.BeginRevision(revisionId);

        definition.PublishRevision(revisionId);

        definition.PublishedRevisionId.Should().Be(revisionId);
        definition.PreparingRevisionId.Should().BeNull();
        definition.IsPreparing.Should().BeFalse();
        definition.HasPublishedRevision.Should().BeTrue();
        definition.RequiresRecalculation.Should().BeFalse();
    }

    [Fact]
    public void DirtyDuringPreparation_RemainsDirtyAfterPublication()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        var revisionId = Guid.NewGuid();
        definition.BeginRevision(revisionId);
        definition.MarkDirty();

        definition.PublishRevision(revisionId);

        definition.IsDirty.Should().BeTrue();
        definition.RequiresRecalculation.Should().BeTrue();
    }

    [Fact]
    public void AbortRevision_ClearsPreparationAndMarksDefinitionDirty()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        var revisionId = Guid.NewGuid();
        definition.BeginRevision(revisionId);

        definition.AbortRevision(revisionId);

        definition.PreparingRevisionId.Should().BeNull();
        definition.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void PublishRevision_WithAnotherRevision_Throws()
    {
        var group = new TestDataSliceGroup("test.group");
        var definition = new TestDataSliceDefinition(group);
        definition.BeginRevision(Guid.NewGuid());

        var action = () => definition.PublishRevision(Guid.NewGuid());

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("The definition does not own the specified preparing revision.");
    }

    private sealed class TestDataSliceGroup(string systemName)
        : DataSliceGroup(systemName);

    private sealed class TestDataSliceDefinition(TestDataSliceGroup group)
        : DataSliceDefinition(group);

    private sealed class TestDataSlice(
        TestDataSliceDefinition definition,
        string? payload = null)
        : DataSlice(definition, payload);
}
