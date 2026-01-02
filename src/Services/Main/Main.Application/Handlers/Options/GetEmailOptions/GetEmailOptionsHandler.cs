using Application.Common.Interfaces;
using Core.Models;

namespace Main.Application.Handlers.Options.GetEmailOptions;

public record GetEmailOptionsQuery() : IQuery<GetEmailOptionsResult>;
public record GetEmailOptionsResult(UserEmailOptions EmailOptions);

public class GetEmailOptionsHandler(UserEmailOptions options) : IQueryHandler<GetEmailOptionsQuery, GetEmailOptionsResult>
{
    public async Task<GetEmailOptionsResult> Handle(GetEmailOptionsQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new GetEmailOptionsResult(options));
    }
}