using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Producers
{
	public class ProducerNotFoundException : NotFoundException
	{
		public ProducerNotFoundException(object key) : base($"Производитель не найден, {key}")
		{
		}
		public ProducerNotFoundException(IEnumerable<int> ids) : base($"Производители не найдены '{string.Join(',', ids)}'")
		{
		}
	}
}
