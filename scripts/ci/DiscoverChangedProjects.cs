#!/usr/bin/env dotnet

using System.Diagnostics;

var output = await GitCommand.RunAsync(
    Directory.GetCurrentDirectory(),
    [
        "diff",
        "--name-only",
        "origin/master...HEAD"
    ]);

Console.WriteLine(output);

public static class GitCommand
{
    public static async Task<string> RunAsync(
        string workingDirectory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
                            ?? throw new InvalidOperationException("Не удалось запустить git.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Git завершился с кодом {process.ExitCode}:{Environment.NewLine}{error}");
        }

        return output;
    }
}