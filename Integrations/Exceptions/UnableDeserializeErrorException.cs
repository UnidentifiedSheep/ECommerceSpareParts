namespace Integrations.Exceptions;

public class UnableDeserializeErrorException(string jsonValue) : Exception(jsonValue, null)
{
}