using Exceptions.Base.Localized;

namespace Pricing.Entities.Exceptions.Markup;

public class MarkupGroupNotFoundException : LocalizedNotFoundException
{
	public MarkupGroupNotFoundException(int id) : base(
		MarkupGroupNotFoundMessage.Instance,
		new
		{
			Id = id
		})
	{
	}
}
