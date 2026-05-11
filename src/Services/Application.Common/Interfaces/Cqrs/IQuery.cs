using MediatR;

namespace Application.Common.Interfaces.Cqrs;

public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull;