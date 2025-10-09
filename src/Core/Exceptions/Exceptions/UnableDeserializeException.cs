using Exceptions.Base;

namespace Exceptions.Exceptions;

public class UnableDeserializeException(string message) : InternalServerException(message);