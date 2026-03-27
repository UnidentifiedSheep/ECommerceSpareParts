using Exceptions.Base;

namespace Search.Abstractions.Exceptions.Suggestions;

public class SuggestionsRebuildingException : ConflictException
{
    public SuggestionsRebuildingException() : base("Автодополнение в процессе перестраивания.")
    {
    }
}