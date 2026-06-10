namespace Application.Common.Interfaces.Lrt;

public interface ILrtDescriptor
{
    Type InputType { get; }
    Type StateType { get; }
}