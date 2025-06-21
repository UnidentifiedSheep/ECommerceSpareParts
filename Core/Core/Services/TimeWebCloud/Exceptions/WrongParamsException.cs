namespace Core.Services.TimeWebCloud.Exceptions;

public class WrongParamsException(string paramName, string message) : ArgumentException(paramName, message)
{
    
}