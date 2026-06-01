using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Options.GetEmailOptions;

public record GetEmailOptionsQuery : IQuery<GetEmailOptionsResult>;

public record GetEmailOptionsResult(UserEmailOptions EmailOptions);

public class GetEmailOptionsHandler(IOptions<UserEmailOptions> options)
    : IQueryHandler<GetEmailOptionsQuery, GetEmailOptionsResult>
{
    public async Task<GetEmailOptionsResult> Handle(GetEmailOptionsQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new GetEmailOptionsResult(options.Value));
    }
}