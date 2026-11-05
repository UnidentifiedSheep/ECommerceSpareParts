using System.ComponentModel.DataAnnotations.Schema;
using Domain;

namespace Tests.Persistence.Entities;

internal sealed class TestMetadataEntity : Entity<TestMetadataEntity, Guid, TestMetadata>
{
	public Guid Id { get; private set; } = Guid.NewGuid();

	public override Guid GetId() => Id;

	public void ReplaceMetadata(TestMetadata metadata) => Metadata = metadata;
}

internal sealed class TestAuditableMetadataEntity : AuditableEntity<TestAuditableMetadataEntity, Guid, TestMetadata>
{
	public Guid Id { get; private set; } = Guid.NewGuid();

	public override Guid GetId() => Id;
}

public sealed class TestMetadata
{
	public string Description { get; set; } = string.Empty;

	public List<string> Tags { get; set; } = [];

	public TestMetadataDetails Details { get; set; } = new();
}

[ComplexType]
public sealed class TestMetadataDetails
{
	public int Quantity { get; set; }
}
