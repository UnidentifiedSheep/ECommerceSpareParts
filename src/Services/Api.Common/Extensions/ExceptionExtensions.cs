using Exceptions.Base;
using FluentValidation;

namespace Api.Common.Extensions;

public static class ExceptionExtensions
{
    public static int GetStatusCode(this Exception exception) => exception switch
    {
        InternalServerException => StatusCodes.Status500InternalServerError,
        ValidationException => StatusCodes.Status400BadRequest,
        BadRequestException => StatusCodes.Status400BadRequest,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        PreconditionRequiredException => StatusCodes.Status428PreconditionRequired,
        _ => StatusCodes.Status500InternalServerError
    };
}