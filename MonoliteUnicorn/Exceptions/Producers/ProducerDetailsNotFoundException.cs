using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers
{
	public class ProducerDetailsNotFoundException : NotFoundException
	{
		public ProducerDetailsNotFoundException(object id) : base("Детали производителя не найдены", new { Id = id })
		{
		}
	}
}
