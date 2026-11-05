using Tests.Persistence.Entities;

namespace Tests.DataBuilders;

internal sealed class TestMetadataEntityBuilder
{
	private readonly TestMetadata _metadata = new();

	public TestMetadataEntityBuilder WithDescription(string description)
	{
		_metadata.Description = description;
		return this;
	}

	public TestMetadataEntityBuilder WithQuantity(int quantity)
	{
		_metadata.Details.Quantity = quantity;
		return this;
	}

	public TestMetadataEntityBuilder WithTags(params string[] tags)
	{
		_metadata.Tags = tags.ToList();
		return this;
	}

	public TestMetadataEntity Build()
	{
		var entity = new TestMetadataEntity();
		entity.ReplaceMetadata(_metadata);
		return entity;
	}
}
