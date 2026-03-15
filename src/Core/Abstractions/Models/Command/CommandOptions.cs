namespace Abstractions.Models.Command;

public class CommandOptions
{
    /// <summary>
    /// Indicates should command save changes at the end. <c>True</c> by default.
    /// </summary>
    public bool SaveChanges { get; protected set; } = true;

    public virtual CommandOptions WithSaveChanges(bool saveChanges = true)
    {
        SaveChanges = saveChanges;
        return this;
    }
}