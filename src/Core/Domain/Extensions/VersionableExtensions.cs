using Domain.Interfaces;
using Exceptions;

namespace Domain.Extensions;

public static class VersionableExtensions
{
    public static void ValidateVersion<TVersion>(this IVersionable<TVersion> versionable, TVersion inputVersion) 
        where TVersion : IComparable
    {
        if (versionable.RowVersion.CompareTo(inputVersion) != 0)
            throw new InvalidRowVersionException();
    }
}