using Api.Common.Hubs;
using Contracts.Job;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Main.Api.Jobs;

public class JobStatusUpdatedConsumer(IHubContext<JobHub> hubContext) : IConsumer<JobStatusUpdatedEvent>
{
	public async Task Consume(ConsumeContext<JobStatusUpdatedEvent> context)
		=> await hubContext.Clients.All.SendAsync("JobStatusUpdated", context.Message);
}
