using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Pricing.Entities;

namespace Main.Migrator.DataSeeds;

public class MarkupGroupSeed : ISeed<UnifiedDbContext>
{
	private static readonly Guid SeedSourceMessageId = new("8f6a0fc3-e7ed-48b9-8716-7bf97e99dd4f");

	public int ExecutionOrder => 1;

	public async Task SeedAsync(UnifiedDbContext context)
	{
		if (await context.Set<MarkupGroup>().AnyAsync(x => x.SourceMessageId != null))
			return;

		var ranges = new[]
		{
			MarkupRange.Create(
				0m,
				100m,
				1.00m),
			MarkupRange.Create(
				100m,
				250m,
				0.90m),
			MarkupRange.Create(
				250m,
				500m,
				0.80m),
			MarkupRange.Create(
				500m,
				1_000m,
				0.70m),
			MarkupRange.Create(
				1_000m,
				2_500m,
				0.60m),
			MarkupRange.Create(
				2_500m,
				5_000m,
				0.50m),
			MarkupRange.Create(
				5_000m,
				10_000m,
				0.40m),
			MarkupRange.Create(
				10_000m,
				25_000m,
				0.30m),
			MarkupRange.Create(
				25_000m,
				50_000m,
				0.20m)
		};

		var group = MarkupGroup.CreateGenerated(
			1,
			ranges,
			SeedSourceMessageId);

		await context.Set<MarkupGroup>().AddAsync(group);
		await context.SaveChangesAsync();
	}
}
