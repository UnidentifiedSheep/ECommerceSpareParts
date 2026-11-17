using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Handlers.ProducerSupplierMappings.GetProducerSupplierMappings;

public class GetProducerSupplierMappingsValidation : AbstractValidator<GetProducerSupplierMappingsQuery>
{
	public GetProducerSupplierMappingsValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
