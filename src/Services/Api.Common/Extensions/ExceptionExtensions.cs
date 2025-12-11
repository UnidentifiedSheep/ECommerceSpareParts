using Core.Abstractions;
using Exceptions.Base;
using Exceptions.Base.Examples;
using FluentValidation;
using NotFoundException = Exceptions.Base.NotFoundException;
using PreconditionRequiredException = Exceptions.Base.PreconditionRequiredException;

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
    
    public static int GetStatusCode(this Type exception)
    {
        if ( exception.IsAssignableTo(typeof(InternalServerException)))
            return StatusCodes.Status500InternalServerError;
        if ( exception.IsAssignableTo(typeof(ValidationException)))
            return StatusCodes.Status400BadRequest;
        if ( exception.IsAssignableTo(typeof(BadRequestException)))
            return StatusCodes.Status400BadRequest;
        if ( exception.IsAssignableTo(typeof(UnauthorizedAccessException)))
            return StatusCodes.Status401Unauthorized;
        if ( exception.IsAssignableTo(typeof(NotFoundException)))
            return StatusCodes.Status404NotFound;
        if ( exception.IsAssignableTo(typeof(ConflictException)))
            return StatusCodes.Status409Conflict;
        if ( exception.IsAssignableTo(typeof(PreconditionRequiredException)))
            return StatusCodes.Status428PreconditionRequired;
        return StatusCodes.Status500InternalServerError;
    }

    public static BaseExceptionExample GetExceptionExample(this Type exceptionType) =>
        exceptionType switch
        {
            _ when exceptionType.IsAssignableTo(typeof(InternalServerException))
                => new InternalServerExample(),

            _ when exceptionType.IsAssignableTo(typeof(ValidationException)) ||
                   exceptionType.IsAssignableTo(typeof(BadRequestException))
                => new BadRequestExample(),

            _ when exceptionType.IsAssignableTo(typeof(UnauthorizedAccessException))
                => new UnauthorizedAccessExample(),

            _ when exceptionType.IsAssignableTo(typeof(NotFoundException))
                => new NotFoundExample(),

            _ when exceptionType.IsAssignableTo(typeof(ConflictException))
                => new ConflictExample(),

            _ when exceptionType.IsAssignableTo(typeof(PreconditionRequiredException))
                => new PreconditionRequiredExample(),

            _ => new InternalServerExample()
        };
}