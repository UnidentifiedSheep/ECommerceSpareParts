using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProducerAliasesImport;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProducerAliasImportLrtTests
    : CsvLrtIntegrationTest<ProducerAliasImportLrt>
{
    public ProducerAliasImportLrtTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task InvalidAndDuplicateAliases_AreReported_AndValidAliasIsCreated()
    {
        var producer = TestContext.Producers[0];
        var existing = await new ProducerAliasBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithAlias("Existing alias")
            .BuildAndAddToDb(Context);

        var execution = await ExecuteCsv(
            "OriginalName,Alias",
            [
                CsvRow(producer.Name, existing.Alias),
                CsvRow(producer.Name, "New alias"),
                CsvRow(producer.Name, "New alias"),
                CsvRow("Unknown producer", "Unknown alias")
            ],
            fileName => new ProducerAliasesImportInputState { FileName = fileName });

        execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
        var state = execution.GetState<ProducerAliasesImportState>();
        state.Errors.Should().HaveCount(3);
        var aliases = await Context.ProducersAliases
            .AsNoTracking()
            .ToListAsync();
        aliases.Should().ContainSingle(x => x.Alias == "NEW ALIAS");
    }
}
