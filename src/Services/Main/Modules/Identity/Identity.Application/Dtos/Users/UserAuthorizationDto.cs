namespace Main.Application.Dtos.Users;

public sealed record UserAuthorizationDto(
	UserDto User,
	IReadOnlyList<string> Roles,
	IReadOnlyList<string> Permissions);
