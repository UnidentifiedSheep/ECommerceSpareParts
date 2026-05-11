namespace Domain.Interfaces;

public interface IVersionable<out TVersion> where TVersion : IComparable
{
    TVersion RowVersion { get; }
}