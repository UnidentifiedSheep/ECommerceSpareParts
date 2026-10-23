using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class ClearExpiredTokensLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ClearExpiredTokensLrt> logger)
	: LrtBase<NoneInputState, NoneInputState>(transactionService, logger)
{
	public static string Name => nameof(ClearExpiredTokensLrt);
	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage
		=> LrtUserTokensClearExpiredNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtUserTokensClearExpiredDescriptionMessage.Instance;
	protected override Task DoWork(NoneInputState inputState)
	{
		return ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			async (ctx, ct) =>
			{
				var now = DateTime.UtcNow;
				var readRepo = ctx.Repositories.GetForRead<UserToken, Guid>();
				var repo = ctx.Repositories.Get<UserToken, Guid>();

				var ids = await readRepo.Query
					.Where(x => x.ExpiresAt <= now)
					.Select(x => x.Id)
					.ToListAsync(ct);

				await repo.DeleteManyAsync(ids, ct);
			});
	}
}
