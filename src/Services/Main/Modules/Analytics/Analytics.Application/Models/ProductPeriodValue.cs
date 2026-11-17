namespace Main.Modules.Analytics.Application.Models;

public sealed record ProductPeriodValue(
	int ProductId,
	DateTime PeriodStart,
	decimal Quantity,
	decimal Revenue);
