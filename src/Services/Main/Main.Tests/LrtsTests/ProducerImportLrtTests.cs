using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProducerImport;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProducerImportLrtTests : CsvLrtIntegrationTest<ProducerImportLrt>
{
	public ProducerImportLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Fact]
	public async Task ExistingAndDuplicateNames_AreSkipped_AndValidProducerIsCreated()
	{
		var newName = $"PRODUCER-{Guid.NewGuid():N}"[..24];
		var execution = await ExecuteCsv(
			"Name,Description",
			[
				CsvRow(TestContext.Producers[0].Name, "Existing"),
				CsvRow(newName, "New"),
				CsvRow(newName, "Duplicate")
			],
			fileName => new ProducerImportInputState
			{
				FileName = fileName
			});

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProducerImportState>();
		state.Errors.Should().ContainSingle(x => x.RowIdx == 3);
		var created = await Context
			.Producers
			.AsNoTracking()
			.CountAsync(x => x.Name == newName.ToUpperInvariant());
		created.Should().Be(1);
	}
}
