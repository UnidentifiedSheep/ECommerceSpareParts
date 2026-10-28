using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.CharacteristicImport;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class CharacteristicImportLrtTests(CombinedContainerFixture fixture)
	: CsvLrtIntegrationTest<CharacteristicImportLrt>(fixture)
{
	[Fact]
	public async Task Import_SkipsNormalizedDuplicates_AndCanBeRepeated()
	{
		await new CharacteristicBuilder(Faker)
			.WithName("Brake Color")
			.BuildAndAddToDb(Context, CancellationToken);

		string[] rows =
		[
			CsvRow("  BRAKE COLOR  "),
			CsvRow("Размер детали"),
			CsvRow("razmer-detali"),
			CsvRow("  Material  "),
			CsvRow("MATERIAL")
		];

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteCsv("Name", rows,
				fileName => new CharacteristicImportInputState { FileName = fileName });

			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
			var state = execution.GetState<CharacteristicImportState>();
			state.Checkpoint!.CurrentLine.Should().Be(5);
			state.Errors.Should().BeEmpty();
		}

		var characteristics = await Context.Characteristics.AsNoTracking().ToListAsync(CancellationToken);
		characteristics.Should().HaveCount(3);
		characteristics.Select(characteristic => characteristic.Name).Should()
			.BeEquivalentTo(["Brake Color", "Размер детали", "Material"]);
		characteristics.Select(characteristic => characteristic.NormalizedName).Should()
			.BeEquivalentTo(["brake-color", "razmer-detali", "material"]);
	}

	[Fact]
	public async Task Import_InvalidAndBoundaryLengthNames_ReportsEachBadRowAndContinues()
	{
		var longestValidName = new string('A', 128);
		var execution = await ExecuteCsv(
			"Name",
			[
				CsvRow("   "),
				CsvRow("!!?"),
				CsvRow(new string('A', 129)),
				CsvRow(longestValidName),
				CsvRow("A"),
				CsvRow("  Material, Grade  ")
			],
			fileName => new CharacteristicImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<CharacteristicImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(6);
		state.Errors.Select(error => error.RowIdx).Should().Equal(1, 2, 3);
		state.Errors.Should().OnlyContain(error => !string.IsNullOrWhiteSpace(error.Message));

		var characteristics = await Context.Characteristics.AsNoTracking().ToListAsync(CancellationToken);
		characteristics.Should().HaveCount(3);
		characteristics.Select(characteristic => characteristic.Name).Should()
			.BeEquivalentTo([longestValidName, "A", "Material, Grade"]);
	}

	[Fact]
	public async Task Import_DuplicateAfterCheckpoint_IsSkippedAgainstSavedCharacteristic()
	{
		var execution = await ExecuteCsv(
			"Name",
			Enumerable.Repeat(CsvRow("Brake Color"), 1000)
				.Append(CsvRow("  BRAKE COLOR  ")),
			fileName => new CharacteristicImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<CharacteristicImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		(await Context.Characteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle(characteristic => characteristic.Name == "Brake Color");
	}

	[Fact]
	public async Task Import_MissingNameColumn_ReportsRowsWithoutCreatingCharacteristics()
	{
		var execution = await ExecuteCsv(
			"OtherColumn",
			[CsvRow("Brake Color"), CsvRow("Material")],
			fileName => new CharacteristicImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<CharacteristicImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(2);
		state.Errors.Select(error => error.RowIdx).Should().Equal(1, 2);
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(0);
	}
}
