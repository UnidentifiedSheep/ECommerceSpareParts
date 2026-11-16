using Main.Entities.User;

namespace Main.Modules.Identity.Application.Models.Users;

public sealed record UserSearchMatch
{
	public required User User { get; init; }

	public required float Rank { get; init; }
}
