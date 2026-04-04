using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Search.Abstractions.Exceptions.Suggestions;

public class SuggestionsRebuildingException() 
    : ConflictException(null), ILocalizableException
{
    public string MessageKey => "suggestion.already.rebuilding";
    public object[]? Arguments => null;
}