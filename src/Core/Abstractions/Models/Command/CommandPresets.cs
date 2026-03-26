namespace Abstractions.Models.Command;

public class CommandPresets
{
    public static readonly CommandOptions WithSaveChanges = new();
    public static readonly CommandOptions WithOutSaveChanges = new CommandOptions().WithSaveChanges(false);
}