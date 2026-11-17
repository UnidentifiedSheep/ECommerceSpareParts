namespace Main.Modules.Identity.Application.Dtos.Users;

public sealed record UserAuthorizationDto(
	UserDto User,
	IReadOnlyList<string> Roles,
	IReadOnlyList<string> Permissions);
