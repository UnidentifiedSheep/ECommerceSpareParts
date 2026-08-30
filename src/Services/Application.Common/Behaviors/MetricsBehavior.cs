using System.Diagnostics;
using Application.Common.Diagnostics;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models;
using MediatR;

namespace Application.Common.Behaviors;

public class MetricsBehavior<TRequest, TResponse>(CqrsMetrics metrics)
	: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : notnull
{
	private static readonly string RequestName = typeof(TRequest).Name;

	// ReSharper disable once StaticMemberInGenericType
	private static readonly string RequestType = GetRequestType();

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		using var activity = CqrsDiagnostics.ActivitySource.StartActivity($"cqrs {RequestName}");

		activity?.SetTag("cqrs.request.name", RequestName);
		activity?.SetTag("cqrs.request.type", RequestType);

		var tags = new TagList
		{
			{
				"cqrs.request.name", RequestName
			},
			{
				"cqrs.request.type", RequestType
			}
		};

		metrics.Requests.Add(1, tags);
		var startedAt = Stopwatch.GetTimestamp();

		try
		{
			return await next(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			metrics.Errors.Add(1, tags);
			activity?.AddEvent(new ActivityEvent("cancelled"));
			throw;
		}
		catch (Exception exception)
		{
			metrics.Errors.Add(1, tags);
			activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
			activity?.AddException(exception);
			throw;
		}
		finally
		{
			var elapsed = Stopwatch.GetElapsedTime(startedAt);
			metrics.Duration.Record(elapsed.TotalMilliseconds, tags);
		}
	}

	private static string GetRequestType()
	{
		if (typeof(TRequest).IsAssignableTo(typeof(ICommand<TResponse>)))
			return "command";

		if (typeof(TRequest).IsAssignableTo(typeof(IQuery<TResponse>)))
			return "query";

		return "unknown";
	}
}
