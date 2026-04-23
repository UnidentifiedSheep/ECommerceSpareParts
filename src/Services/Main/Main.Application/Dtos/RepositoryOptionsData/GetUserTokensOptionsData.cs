using Main.Enums;

namespace Main.Application.Dtos.RepositoryOptionsData;

public record GetUserTokensOptionsData
{
    public required Guid UserId { get; init; }
    public TokenType? TokenType { get; init; }
}