using MediatR;

namespace Main.Application.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull;