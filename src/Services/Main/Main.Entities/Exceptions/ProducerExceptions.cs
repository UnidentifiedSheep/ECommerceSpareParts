using Exceptions.Base.Localized;
using Enums;

namespace Main.Entities.Exceptions;

public class CannotDeleteProducerWithArticlesException()
    : LocalizedBadRequestException("producer.with.articles.cannot.be.deleted");

public class ProducerNotFoundException(int id)
    : LocalizedNotFoundException("producer.not.found", new { Id = id });

public class ProducersAliasNotFoundException(string name)
    : LocalizedNotFoundException(
        "producer.additional.name.not.found",
        new { Name = name },
        [name]);

public class ProducersSupplierMappingNotFoundException(int id)
    : LocalizedNotFoundException(
        "producer.supplier.mapping.not.found",
        new { Id = id });

public class ProducersSupplierMappingAlreadyExistsException(int producerId, Supplier supplier)
    : LocalizedConflictException(
        "producer.supplier.mapping.already.exists",
        new { ProducerId = producerId, Supplier = supplier });
