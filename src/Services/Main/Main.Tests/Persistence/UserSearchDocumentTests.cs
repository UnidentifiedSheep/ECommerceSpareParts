using Application.Common.Models;
using FluentAssertions;
using Main.Application.Handlers.Users.SearchUsers;
using Main.Entities.User;
using Main.Enums;
using Main.Persistence.DatabaseObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class UserSearchDocumentTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task SourceChanges_UpdateProjectionAndContactDeletionRemovesTerms()
	{
		var user = await CreateUser("original-login", "Уникальноеимя", "Начальная");
		user.AddEmail("original-contact@contacts.example", EmailType.Unknown, false, false);
		user.AddUserPhone("79994445566", PhoneType.Unknown, false, false);
		await Context.SaveChangesAsync(CancellationToken);
		(await Search("Уникальноеимя")).Users.Should().ContainSingle();

		user.SetUserInfo("Обновленноеимя", "Новаяфамилия", null);
		await Context.SaveChangesAsync(CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"UPDATE auth.users SET user_name = 'changed-login', normalized_user_name = 'CHANGED-LOGIN' WHERE id = {user.Id}", CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"UPDATE auth.user_emails SET normalized_email = 'changed-contact@contacts.example' WHERE user_id = {user.Id}", CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"UPDATE auth.user_phones SET normalized_phone = '79995556677', phone_number = '79995556677' WHERE user_id = {user.Id}", CancellationToken);

		(await Search("Уникальноеимя")).Users.Should().BeEmpty();
		(await Search("original-contact@contacts.example")).Users.Should().BeEmpty();
		(await Search("79994445566")).Users.Should().BeEmpty();
		(await Search("Обновленноеимя")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("changed-login")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("changed-contact@contacts.example")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("79995556677")).Users.Select(result => result.Id).Should().Equal(user.Id);

		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"DELETE FROM auth.user_emails WHERE user_id = {user.Id}", CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"DELETE FROM auth.user_phones WHERE user_id = {user.Id}", CancellationToken);
		(await Search("changed-contact@contacts.example")).Users.Should().BeEmpty();
		(await Search("79995556677")).Users.Should().BeEmpty();

		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"DELETE FROM auth.user_info WHERE user_id = {user.Id}", CancellationToken);
		(await Search("Обновленноеимя")).Users.Should().BeEmpty();
		(await Search("changed-login")).Users.Should().ContainSingle();

		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"DELETE FROM auth.organization_members WHERE user_id = {user.Id}", CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"DELETE FROM auth.users WHERE id = {user.Id}", CancellationToken);
		(await Context.Database.SqlQuery<int>(
			$"SELECT count(*)::integer AS \"Value\" FROM auth.user_search WHERE user_id = {user.Id}")
			.SingleAsync(CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task ProjectionChanges_RollBackWithSourceTransaction()
	{
		var user = await CreateUser("rollback-original", "Александр", "Петров");
		await using var transaction = await Context.Database.BeginTransactionAsync(CancellationToken);
		await Context.Database.ExecuteSqlInterpolatedAsync(
			$"UPDATE auth.users SET normalized_user_name = 'ROLLBACK-TEMPORARY' WHERE id = {user.Id}", CancellationToken);
		(await Search("rollback-temporary")).Users.Should().ContainSingle();

		await transaction.RollbackAsync(CancellationToken);

		(await Search("rollback-temporary")).Users.Should().BeEmpty();
		(await Search("rollback-original")).Users.Should().ContainSingle();
	}

	[Fact]
	public async Task DatabaseObjects_BackfillExistingUsersAndSupportRecreation()
	{
		await ExecuteStatements(UserSearchSql.V1.DropTriggers, UserSearchSql.V1.DropFunctions);
		await Context.Database.ExecuteSqlRawAsync("DELETE FROM auth.user_search", CancellationToken);
		var user = await CreateUser("migration-existing", "Миграционный", "Пользователь", "Existing profile");
		user.AddEmail("migration.contact@contacts.example", EmailType.Unknown, false, false);
		await Context.SaveChangesAsync(CancellationToken);

		await ExecuteStatements(
			UserSearchSql.V1.CreateFunctions,
			UserSearchSql.V1.CreateTriggers,
			UserSearchSql.V1.Backfill);

		(await Search("Миграционный")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("migration.contact@contacts.example")).Users.Select(result => result.Id).Should().Equal(user.Id);
		await ExecuteStatements(UserSearchSql.V1.DropTriggers, UserSearchSql.V1.DropFunctions);
		await ExecuteStatements(
			UserSearchSql.V1.CreateFunctions,
			UserSearchSql.V1.CreateTriggers,
			UserSearchSql.V1.Backfill);
		(await Search("Миграционный")).Users.Should().ContainSingle();
	}

	[Fact]
	public async Task ConcurrentChangesInDifferentTables_DoNotLoseSearchTerms()
	{
		var user = await CreateUser("concurrent-user", "Первоначальное", "Имя");
		user.AddEmail("concurrent.old@contacts.example", EmailType.Unknown, false, false);
		await Context.SaveChangesAsync(CancellationToken);
		await using var first = new NpgsqlConnection(Context.Database.GetConnectionString());
		await using var second = new NpgsqlConnection(Context.Database.GetConnectionString());
		await first.OpenAsync(CancellationToken);
		await second.OpenAsync(CancellationToken);
		await using var firstTransaction = await first.BeginTransactionAsync(CancellationToken);
		await using var secondTransaction = await second.BeginTransactionAsync(CancellationToken);
		await using var profileCommand = new NpgsqlCommand(
			"UPDATE auth.user_info SET name = 'Параллельноеимя' WHERE user_id = @id", first, firstTransaction);
		profileCommand.Parameters.AddWithValue("id", user.Id);
		await profileCommand.ExecuteNonQueryAsync(CancellationToken);
		await using var emailCommand = new NpgsqlCommand(
			"UPDATE auth.user_emails SET normalized_email = 'concurrent.new@contacts.example' WHERE user_id = @id",
			second, secondTransaction);
		emailCommand.Parameters.AddWithValue("id", user.Id);
		var emailUpdate = emailCommand.ExecuteNonQueryAsync(CancellationToken);

		await using var observer = new NpgsqlConnection(Context.Database.GetConnectionString());
		await observer.OpenAsync(CancellationToken);
		await using var lockCommand = new NpgsqlCommand(
			"SELECT coalesce(wait_event_type = 'Lock', false) FROM pg_stat_activity WHERE pid = @pid", observer);
		lockCommand.Parameters.AddWithValue("pid", second.ProcessID);
		var deadline = DateTime.UtcNow.AddSeconds(10);
		var waiting = false;
		while (!waiting && DateTime.UtcNow < deadline)
		{
			waiting = (bool)(await lockCommand.ExecuteScalarAsync(CancellationToken))!;
			if (!waiting) await Task.Delay(20, CancellationToken);
		}
		await firstTransaction.CommitAsync(CancellationToken);
		await emailUpdate;
		await secondTransaction.CommitAsync(CancellationToken);

		waiting.Should().BeTrue("updates to one user's search document must be serialized");
		(await Search("Параллельноеимя")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("concurrent.new@contacts.example")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("concurrent.old@contacts.example")).Users.Should().BeEmpty();
	}

	private Task<SearchUsersResult> Search(string term, Pagination? pagination = null) =>
		Mediator.Send(new SearchUsersQuery(term, pagination ?? new Pagination(0, 25)), CancellationToken);

	private Task<User> CreateUser(string login, string name, string surname, string? description = null) =>
		new UserBuilder(Faker).WithUserName(login).WithUserInfo(name, surname, description)
			.BuildAndAddToDb(Context);

	private async Task ExecuteStatements(params string[] statements)
	{
		foreach (var sql in statements)
			await Context.Database.ExecuteSqlRawAsync(sql, CancellationToken);
	}
}
