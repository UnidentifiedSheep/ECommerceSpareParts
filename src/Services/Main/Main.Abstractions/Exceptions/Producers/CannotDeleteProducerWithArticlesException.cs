using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Producers;

public class CannotDeleteProducerWithArticlesException() : BadRequestException(null), ILocalizableException
{
    public string MessageKey => "producer.with.articles.cannot.be.deleted";
    public object[]? Arguments => null;
}