using System.Reflection;
using Abstractions.Interfaces.Persistence;
using Attributes;
using MediatR;

namespace Application.Common.Behaviors;

public class EventSideEffectsSaveChangesBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : SaveChangesBehavior<TRequest, TResponse>(unitOfWork)
    where TRequest : IRequest<TResponse>
    where TResponse : notnull { }
