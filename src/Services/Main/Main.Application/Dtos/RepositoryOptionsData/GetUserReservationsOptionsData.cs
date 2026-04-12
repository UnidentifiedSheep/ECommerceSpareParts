namespace Main.Abstractions.Dtos.RepositoryOptionsData;

public record GetUserReservationsOptionsData
{
    public required Guid UserId { get; init; }
    public required IReadOnlyList<int> ArticleIds { get; init; }
    public bool IsDone { get; init; }
}