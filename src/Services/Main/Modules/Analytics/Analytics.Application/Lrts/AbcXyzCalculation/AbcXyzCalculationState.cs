using Application.Common.Interfaces;
using FluentValidation;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Analyzers.AbcXyz;

namespace Main.Modules.Analytics.Application.Lrts.AbcXyzCalculation;

public record AbcXyzCalculationState : AbcXyzCalculationInputState;

public record AbcXyzCalculationInputState : AbcXyzParameters, IInputState
{
	public void ValidateState() => new AbcXyzParametersValidator().ValidateAndThrow(this);
}
