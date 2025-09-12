using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles
{
	public class CharacteristicsNotFoundException : NotFoundException
	{
		public CharacteristicsNotFoundException(object key) : base("Характеристики артикула не были найдены", key)
		{
		}
	}
}
