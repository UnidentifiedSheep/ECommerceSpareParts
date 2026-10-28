using Exceptions;
using FluentAssertions;
using Pricing.Entities;

namespace Pricing.Integration.Tests.Domain;

public sealed class MarkupGroupTests
{
	[Theory]
	[InlineData(0, 10, 5, 15)]
	[InlineData(0, 100, 10, 20)]
	[InlineData(10, 20, 0, 100)]
	[InlineData(0, 10, 0, 10)]
	[InlineData(0, 10, 0, 20)]
	[InlineData(0, 10, 5, 5)]
	[InlineData(5, 5, 5, 5)]
	[InlineData(5, 5, 5, 10)]
	public void Create_OverlappingRanges_AreRejected(int start, int end, int otherStart, int otherEnd)
	{
		var action = () => MarkupGroup.Create("Test", 1,
		[
			MarkupRange.Create(start, end, 0.2m),
			MarkupRange.Create(otherStart, otherEnd, 0.3m)
		]);

		action.Should().Throw<InvalidInputException>().Which.LocalizableMessage.MessageKey
			.Should().Be("markup.group.ranges.must.not.overlap");
	}

	[Fact]
	public void Create_UnsortedAdjacentRanges_AreAllowed()
	{
		var group = MarkupGroup.Create("Test", 1,
		[
			MarkupRange.Create(10m, 20m, 0.2m),
			MarkupRange.Create(0m, 10m, 0.3m),
			MarkupRange.Create(20m, 30m, 0.1m)
		]);

		group.MarkupRanges.Should().HaveCount(3);
	}

	[Fact]
	public void Create_SinglePointRange_IsAllowed()
	{
		var group = MarkupGroup.Create("Test", 1, [MarkupRange.Create(5m, 5m, 0.2m)]);

		group.MarkupRanges.Should().ContainSingle();
	}

	[Fact]
	public void Update_OverlappingRanges_LeavesGroupUnchanged()
	{
		var range = MarkupRange.Create(0m, 100m, 0.2m);
		var group = MarkupGroup.Create("Original", 1, [range]);

		var action = () => group.Update("Changed", 2,
		[
			MarkupRange.Create(0m, 10m, 0.2m),
			MarkupRange.Create(5m, 20m, 0.3m)
		]);

		action.Should().Throw<InvalidInputException>();
		group.Name.Should().Be("Original");
		group.CurrencyId.Should().Be(1);
		group.MarkupRanges.Should().Equal(range);
	}
}
