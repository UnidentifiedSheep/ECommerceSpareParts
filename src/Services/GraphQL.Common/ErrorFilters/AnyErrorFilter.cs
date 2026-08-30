using Exceptions.Base;
using HotChocolate.Execution;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public sealed class AnyErrorFilter(
	ILoggerFactory loggerFactory,
	IContextualStringLocalizer localizer,
	IHttpContextAccessor httpContextAccessor)
	: GraphQlErrorFilterBase<AnyErrorFilter, Exception>(loggerFactory, localizer, httpContextAccessor)
{
	protected override IError Handle(IError error, Exception exception)
	{
		var status = GetStatusCode(exception);
		var isInternalError = status == StatusCodes.Status500InternalServerError;
		var fallback = string.IsNullOrWhiteSpace(exception.Message)
			? exception.GetType().Name
			: exception.Message;
		var message = isInternalError
			? nameof(InternalServerException)
			: GetLocalizedMessage(exception, fallback);
		var code = isInternalError ? "INTERNAL_SERVER_ERROR" : exception.GetType().Name;

		var builder = CreateErrorBuilder(error, exception, message, code, status);
		AddExceptionRelatedData(builder, exception);

		return builder.Build();
	}
}
