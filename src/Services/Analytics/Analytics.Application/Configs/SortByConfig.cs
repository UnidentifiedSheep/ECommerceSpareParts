using Abstractions;
using Application.Common.Extensions;
using Application.Common.Querying;

namespace Analytics.Application.Configs;

public static class SortByConfig
{
	public static void Configure() => QueryableSortBy.Value.ConfigureForJob();
}
