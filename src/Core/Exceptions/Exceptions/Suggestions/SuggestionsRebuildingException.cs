using Exceptions.Base;

namespace Exceptions.Exceptions.Suggestions;

public class SuggestionsRebuildingException : ConflictException
{
    public SuggestionsRebuildingException() : base("Автодополнение в процессе перестраивания.")
    {
    }
}