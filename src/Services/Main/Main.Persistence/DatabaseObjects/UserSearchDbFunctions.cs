using NpgsqlTypes;

namespace Main.Persistence.DatabaseObjects;

public static class UserSearchDbFunctions
{
	public static NpgsqlTsQuery ToUserSearchQuery(string searchTerm) =>
		throw new NotSupportedException("This function can only be used in database queries.");
}
