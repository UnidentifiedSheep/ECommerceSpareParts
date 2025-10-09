namespace Exceptions.Exceptions;

public class MappingAlreadyExists(object? key) : Exception($"Mapping {key} already exists");