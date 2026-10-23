using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class ClearExpiredTokensLrtTests(CombinedContainerFixture fixture)
	: LrtIntegrationTest<ClearExpiredTokensLrt>(fixture)
{
	[Fact]
	public async Task Execute_RemovesExpiredTokensAndPreservesUnexpiredAndNonExpiringTokens()
	{
		var user = await new MemberUserBuilder(Faker).BuildAndAddToDb(Context);
		var now = DateTime.UtcNow;
		await AddTokenAsync(user.Id, now.AddDays(-1));
		await AddTokenAsync(user.Id, now.AddSeconds(-1), revoked: true);
		var active = await AddTokenAsync(user.Id, now.AddDays(1));
		var revoked = await AddTokenAsync(user.Id, now.AddDays(1), revoked: true);
		var noExpiry = await AddTokenAsync(user.Id, null);
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteLrt(cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var remaining = await Context.UserTokens.AsNoTracking().Select(x => x.Id).ToListAsync(CancellationToken);
		remaining.Should().BeEquivalentTo([active.Id, revoked.Id, noExpiry.Id]);
	}

	[Fact]
	public async Task Execute_EmptyTableAndRepeatedCleanupSucceed()
	{
		var first = await ExecuteLrt(cancellationToken: CancellationToken);
		var second = await ExecuteLrt(cancellationToken: CancellationToken);

		first.Job.Status.Should().Be(JobStatus.Succeeded, first.Job.ErrorMessage);
		second.Job.Status.Should().Be(JobStatus.Succeeded, second.Job.ErrorMessage);
		(await Context.UserTokens.CountAsync(CancellationToken)).Should().Be(0);
	}

	private async Task<UserToken> AddTokenAsync(Guid userId, DateTime? expiresAt, bool revoked = false)
	{
		var token = UserToken.CreateRefreshToken(userId, Guid.NewGuid().ToString("N"));
		await Context.UserTokens.AddAsync(token, CancellationToken);
		Context.Entry(token).Property(x => x.ExpiresAt).CurrentValue = expiresAt;
		Context.Entry(token).Property(x => x.Revoked).CurrentValue = revoked;
		return token;
	}
}
