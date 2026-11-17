namespace Application.Common.Interfaces;

public interface ILrtDescriptor
{
	Type InputType { get; }

	Type StateType { get; }

	Type? OutputType => null;
}
