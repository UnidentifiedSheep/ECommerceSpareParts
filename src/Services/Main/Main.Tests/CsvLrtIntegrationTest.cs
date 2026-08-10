using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Main.Application.Lrts.Base;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests;

public abstract class CsvLrtIntegrationTest<TLrt>(CombinedContainerFixture fixture)
    : LrtIntegrationTest<TLrt>(fixture)
    where TLrt : class, ILrtNamedObject
{
    private const string UploadsBucket = "uploads";

    protected Task<LrtExecutionResult> ExecuteCsv<TInputState>(
        string header,
        IEnumerable<string> rows,
        Func<string, TInputState> createState)
        where TInputState : ICsvImportInputState
    {
        var fileName = $"{Guid.NewGuid():N}.csv";
        var csv = string.Join(
            Environment.NewLine,
            new[] { header }.Concat(rows));
        Scope.ServiceProvider
            .GetRequiredService<S3StorageServiceStub>()
            .SetFile(UploadsBucket, fileName, csv);

        return ExecuteLrt(JsonSerializer.Serialize(createState(fileName)));
    }

    protected static string CsvRow(params object?[] values)
        => string.Join(",", values.Select(x => Escape(x?.ToString() ?? string.Empty)));

    private static string Escape(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";
}
