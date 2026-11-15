using Application.Common.Querying;
using Main.Entities.Sale;

namespace Main.Application.Configs;

public static class SaleSortByConfig
{
	public static void Configure()
		=> QueryableSortBy
			.Value
			.MapDefault<Sale, DateTime>(x => x.SaleDatetime, true)
			.Map<Sale, DateTime>("dateTime", x => x.SaleDatetime)
			.Map<Sale, decimal>("totalSum", x => x.Contents.Sum(content => content.TotalSum))
			.Map<Sale, Guid>("id", x => x.Id);
}
