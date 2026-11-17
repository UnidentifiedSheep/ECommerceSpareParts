using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Models;

namespace Main.Modules.Analytics.Application.Interfaces;

public interface IAbcXyzCalculator
{
	AbcXyzAnalysisResult Calculate(IEnumerable<ProductPeriodValue> rows, AbcXyzParameters parameters);
}
