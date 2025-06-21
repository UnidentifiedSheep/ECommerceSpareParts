using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers
{
	public class ProducerDetailsNotFoundException : NotFoundException
	{
		public ProducerDetailsNotFoundException(object key) : base("Детали производителя не найдены", key)
		{
		}
	}
}
