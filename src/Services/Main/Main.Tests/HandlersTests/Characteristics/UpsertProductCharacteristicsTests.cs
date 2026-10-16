using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductCharacteristics.UpsertCharacteristics;
using Main.Entities.Product;
using Moq;

namespace Tests.HandlersTests.Characteristics;

public class UpsertProductCharacteristicsTests
{
	[Fact]
	public async Task MixedBatch_UpdatesCreatesAndCanBeRepeated()
	{
		var existing = ProductCharacteristic.Create(1, 10, "Old");
		var untouched = ProductCharacteristic.Create(1, 20, "Unchanged");
		var stored = new List<ProductCharacteristic> { existing, untouched };
		var repository = new Mock<IRepository<ProductCharacteristic, (int, int)>>();
		repository.Setup(x => x.ListAsync(
				It.Is<Criteria<ProductCharacteristic>>(criteria => criteria.Track),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(stored);
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.Setup(x => x.AddRangeAsync(
				It.IsAny<IEnumerable<ProductCharacteristic>>(), It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<ProductCharacteristic>, CancellationToken>((items, _) => stored.AddRange(items))
			.Returns(Task.CompletedTask);
		var handler = new UpsertCharacteristicsHandler(repository.Object, unitOfWork.Object);
		var command = new UpsertCharacteristicsCommand(
		[
			new UpsertCharacteristicsDto { ProductId = 1, CharacteristicId = 10, Value = "  Updated  " },
			new UpsertCharacteristicsDto { ProductId = 2, CharacteristicId = 20, Value = "  New  " }
		]);

		await handler.Handle(command, CancellationToken.None);
		await handler.Handle(command, CancellationToken.None);

		existing.Value.Should().Be("Updated");
		untouched.Value.Should().Be("Unchanged");
		stored.Should().HaveCount(3);
		stored.Single(x => x.GetId() == (2, 20)).Value.Should().Be("New");
		unitOfWork.Verify(x => x.AddRangeAsync(
			It.IsAny<IEnumerable<ProductCharacteristic>>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public void DuplicatePairs_ReturnValidationError()
	{
		var command = new UpsertCharacteristicsCommand(
		[
			new UpsertCharacteristicsDto { ProductId = 1, CharacteristicId = 10, Value = "First" },
			new UpsertCharacteristicsDto { ProductId = 1, CharacteristicId = 10, Value = "Second" }
		]);

		new UpsertCharacteristicsValidation().Validate(command).IsValid.Should().BeFalse();
	}

	[Theory]
	[InlineData(null, false)]
	[InlineData("", false)]
	[InlineData("   ", false)]
	[InlineData("  ab  ", true)]
	[InlineData("  Red  ", true)]
	public void ValueValidation_HandlesMissingAndTrimmedValues(string? value, bool expected)
	{
		var command = new UpsertCharacteristicsCommand(
		[
			new UpsertCharacteristicsDto { ProductId = 1, CharacteristicId = 10, Value = value! }
		]);

		new UpsertCharacteristicsValidation().Validate(command).IsValid.Should().Be(expected);
	}
}
