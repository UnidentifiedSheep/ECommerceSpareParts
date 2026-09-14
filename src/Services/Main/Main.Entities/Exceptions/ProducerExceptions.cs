using Enums;
using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CannotDeleteProducerWithArticlesException()
	: LocalizedBadRequestException(ProducerWithArticlesCannotBeDeletedMessage.Instance);

public class ProducerNotFoundException(int id) : LocalizedNotFoundException(
	ProducerNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class ProducersAliasNotFoundException(string name) : LocalizedNotFoundException(
	new ProducerAdditionalNameNotFoundMessage().WithName(name),
	new
	{
		Name = name
	});

public class ProducersSupplierMappingNotFoundException(int id) : LocalizedNotFoundException(
	ProducerSupplierMappingNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class ProducersSupplierMappingAlreadyExistsException(string supplierProducerName, Supplier supplier)
	: LocalizedConflictException(
		ProducerSupplierMappingAlreadyExistsMessage.Instance,
		new
		{
			SupplierProducerName = supplierProducerName, Supplier = supplier
		});
