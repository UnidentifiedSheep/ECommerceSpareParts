using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions
{
	public class CharacteristicsNotFoundException : NotFoundException
	{
		public CharacteristicsNotFoundException(object key) : base("Характеристики артикула не были найдены", key)
		{
		}
	}
}
