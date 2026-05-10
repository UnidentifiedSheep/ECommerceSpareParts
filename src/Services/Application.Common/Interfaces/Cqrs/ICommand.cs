using MediatR;

namespace Application.Common.Interfaces.Cqrs;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;