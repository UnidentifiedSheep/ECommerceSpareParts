using MediatR;

namespace Application.Interfaces;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;