namespace Core.Exceptions;

public class MappingDoesntExists(object? key) : Exception($"Mapping mapping doesn't exists for key: {key}");