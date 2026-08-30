namespace Application.Common.Interfaces.Lrt;

public interface IMultiStepLrt : ILrtNamedObject
{
	void ConfigureSteps(IMultiStepJobBuilder builder, string initialState);
}
