using Abstractions;
using Application.Common.Extensions;

namespace Analytics.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortBy.Value.ConfigureForJob();
    }
}
