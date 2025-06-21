using Core.Services.TimeWebCloud.Models;

namespace Core.Services.TimeWebCloud.Exceptions;

public class NotSuccessfulRequestException(ExceptionModel model) : Exception()
{
    public ExceptionModel Error { get; } = model;
}