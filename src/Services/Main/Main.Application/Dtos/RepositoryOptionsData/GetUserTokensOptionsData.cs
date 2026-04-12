using Main.Enums;

namespace Main.Abstractions.Dtos.RepositoryOptionsData;

public record GetUserTokensOptionsData
{
    public required Guid UserId { get; init; }
    public TokenType? TokenType { get; init; }
}