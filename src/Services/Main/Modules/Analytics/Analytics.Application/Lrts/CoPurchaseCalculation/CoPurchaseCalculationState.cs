using Application.Common.Interfaces;
using FluentValidation;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Analyzers.CoPurchase;

namespace Main.Modules.Analytics.Application.Lrts.CoPurchaseCalculation;

public record CoPurchaseCalculationState : CoPurchaseCalculationInputState
{
	public Guid? RunId { get; init; }

	public int TotalSalesCount { get; init; }

	public int LastProductId { get; init; }

	public int ProductsCount { get; init; }

	public int ProductsWithRelatedCount { get; init; }
}

public record CoPurchaseCalculationInputState : CoPurchaseParameters, IInputState
{
	public void ValidateState() => new CoPurchaseParametersValidator().ValidateAndThrow(this);
}
