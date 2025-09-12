using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Producers
{
	public class ProducerDetailsNotFoundException : NotFoundException
	{
		public ProducerDetailsNotFoundException(object id) : base("Детали производителя не найдены", new { Id = id })
		{
		}
	}
}
