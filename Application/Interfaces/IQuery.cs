using MediatR;

namespace Application.Interfaces
{
	public interface IQuery<out TResponse> : IRequest<TResponse> 
		where TResponse : notnull;
}
