using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers
{
	public class ProducerNotFoundException : NotFoundException
	{
		public ProducerNotFoundException(object id) : base($"Производитель не найден", new { Id = id })
		{
		}
		public ProducerNotFoundException(IEnumerable<int> ids) : base($"Производители не найдены", new { Ids = ids })
		{
		}
	}
}
