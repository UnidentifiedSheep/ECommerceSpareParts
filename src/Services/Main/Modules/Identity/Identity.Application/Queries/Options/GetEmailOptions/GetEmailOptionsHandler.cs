using Application.Common.Interfaces.Cqrs;
using Main.Modules.Identity.Application.Models.Options;
using Microsoft.Extensions.Options;

namespace Main.Modules.Identity.Application.Queries.Options.GetEmailOptions;

public record GetEmailOptionsQuery : IQuery<GetEmailOptionsResult>;

public record GetEmailOptionsResult(UserEmailOptions EmailOptions);

public class GetEmailOptionsHandler(IOptions<UserEmailOptions> options)
	: IQueryHandler<GetEmailOptionsQuery, GetEmailOptionsResult>
{
	public async Task<GetEmailOptionsResult> Handle(
		GetEmailOptionsQuery request,
		CancellationToken cancellationToken)
		=> await Task.FromResult(new GetEmailOptionsResult(options.Value));
}
