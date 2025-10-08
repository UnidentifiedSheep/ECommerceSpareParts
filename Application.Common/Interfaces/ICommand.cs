using MediatR;

namespace Application.Common.Interfaces;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;