using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProductGroupImport;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class ProductGroupImportLrtTests(CombinedContainerFixture fixture)
	: CsvLrtIntegrationTest<ProductGroupImportLrt>(fixture)
{
	[Fact]
	public async Task Import_ValidatesNames_AndSkipsNormalizedDuplicates()
	{
		await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context);

		var execution = await ExecuteCsv(
			"Name",
			[
				CsvRow("  BRAKE PADS  "),
				CsvRow("Oil Filters"),
				CsvRow("  OIL FILTERS  "),
				CsvRow("ab"),
				CsvRow("Air Filters")
			],
			fileName => new ProductGroupImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductGroupImportState>();
		state.CurrentLine.Should().Be(5);
		state.SkippedLines.Should().BeEquivalentTo([1, 3]);
		state.Errors.Should().ContainSingle(error => error.RowIdx == 4);

		var groups = await Context.ProductGroups.AsNoTracking().ToListAsync(CancellationToken);
		groups.Should().HaveCount(3);
		groups.Select(group => group.NormalizedName).Should()
			.BeEquivalentTo(["brake-pads", "oil-filters", "air-filters"]);
	}

	[Fact]
	public async Task Import_InvalidAndBoundaryLengthNames_ReportsEachBadRowAndContinues()
	{
		var longestValidName = new string('A', 256);
		var execution = await ExecuteCsv(
			"Name",
			[
				CsvRow("   "),
				CsvRow("!!?"),
				CsvRow(new string('A', 257)),
				CsvRow(longestValidName),
				CsvRow("  Filters, Oil  ")
			],
			fileName => new ProductGroupImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductGroupImportState>();
		state.CurrentLine.Should().Be(5);
		state.Errors.Select(error => error.RowIdx).Should().Equal(1, 2, 3);
		state.Errors.Should().OnlyContain(error => !string.IsNullOrWhiteSpace(error.Message));
		state.SkippedLines.Should().BeEmpty();

		var groups = await Context.ProductGroups.AsNoTracking().ToListAsync(CancellationToken);
		groups.Should().HaveCount(2);
		groups.Select(group => group.Name).Should()
			.BeEquivalentTo([longestValidName, "Filters, Oil"]);
	}

	[Fact]
	public async Task Import_DuplicateAfterCheckpoint_IsSkippedAgainstSavedGroup()
	{
		var execution = await ExecuteCsv(
			"Name",
			Enumerable.Repeat(CsvRow("Brake Pads"), 1000)
				.Append(CsvRow("  BRAKE PADS  ")),
			fileName => new ProductGroupImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductGroupImportState>();
		state.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		state.SkippedLines.Should().BeEquivalentTo(Enumerable.Range(2, 1000));
		(await Context.ProductGroups.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle(group => group.Name == "Brake Pads");
	}

	[Fact]
	public async Task Import_MissingNameColumn_ReportsRowsWithoutCreatingGroups()
	{
		var execution = await ExecuteCsv(
			"OtherColumn",
			[CsvRow("Brake Pads"), CsvRow("Oil Filters")],
			fileName => new ProductGroupImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductGroupImportState>();
		state.CurrentLine.Should().Be(2);
		state.Errors.Select(error => error.RowIdx).Should().Equal(1, 2);
		state.SkippedLines.Should().BeEmpty();
		(await Context.ProductGroups.CountAsync(CancellationToken)).Should().Be(0);
	}
}
