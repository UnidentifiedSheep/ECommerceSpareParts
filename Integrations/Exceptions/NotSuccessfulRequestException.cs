using Integrations.Models.TimeWebCloud;

namespace Integrations.Exceptions;

public class NotSuccessfulRequestException(ExceptionModel model) : Exception
{
    public ExceptionModel Error { get; } = model;
}