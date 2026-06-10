namespace Application.Common.Interfaces.Lrt;

public interface IInputState
{
    public static abstract string GetAndValidateState(string jsonState);
}