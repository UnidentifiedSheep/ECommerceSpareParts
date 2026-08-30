using Enums;
using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CannotDeleteProducerWithArticlesException()
	: LocalizedBadRequestException("producer.with.articles.cannot.be.deleted");

public class ProducerNotFoundException(int id) : LocalizedNotFoundException(
	"producer.not.found",
	new
	{
		Id = id
	});

public class ProducersAliasNotFoundException(string name) : LocalizedNotFoundException(
	"producer.additional.name.not.found",
	new
	{
		Name = name
	},
	[name]);

public class ProducersSupplierMappingNotFoundException(int id) : LocalizedNotFoundException(
	"producer.supplier.mapping.not.found",
	new
	{
		Id = id
	});

public class ProducersSupplierMappingAlreadyExistsException(string supplierProducerName, Supplier supplier)
	: LocalizedConflictException(
		"producer.supplier.mapping.already.exists",
		new
		{
			SupplierProducerName = supplierProducerName, Supplier = supplier
		});
