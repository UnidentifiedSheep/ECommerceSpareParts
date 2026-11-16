using NpgsqlTypes;

namespace Main.Modules.Identity.Persistence.DatabaseObjects;

public static class UserSearchDbFunctions
{
	public static NpgsqlTsQuery ToUserSearchQuery(string searchTerm)
		=> throw new NotSupportedException("This function can only be used in database queries.");
}
