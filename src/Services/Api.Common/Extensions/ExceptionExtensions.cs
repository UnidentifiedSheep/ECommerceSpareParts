using Exceptions.Base;
using FluentValidation;
using NotFoundException = Exceptions.Base.NotFoundException;
using PreconditionRequiredException = Exceptions.Base.PreconditionRequiredException;

namespace Api.Common.Extensions;

public static class ExceptionExtensions
{
    public static int GetStatusCode(this Exception exception)
    {
        switch (exception)
        {
            case BulkValidation.Core.Exceptions.ValidationException bv:
            {
                int max = -1;

                foreach (var failure in bv.Failures)
                    if (failure.ErrorCode.HasValue && failure.ErrorCode.Value > max)
                        max = failure.ErrorCode.Value;
                
                return max == -1 ? StatusCodes.Status500InternalServerError : max;
            }

            case ValidationException:
            case BadRequestException:
                return StatusCodes.Status400BadRequest;

            case UnauthorizedAccessException:
                return StatusCodes.Status401Unauthorized;

            case NotFoundException:
                return StatusCodes.Status404NotFound;

            case ConflictException:
                return StatusCodes.Status409Conflict;

            case PreconditionRequiredException:
                return StatusCodes.Status428PreconditionRequired;

            case InternalServerException:
                return StatusCodes.Status500InternalServerError;

            default:
                return StatusCodes.Status500InternalServerError;
        }
    }
}