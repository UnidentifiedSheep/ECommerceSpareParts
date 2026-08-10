using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Application.Common.Dtos;
using Application.Common.Handlers.JobSchedules;
using Application.Common.Handlers.JobSchedules.CreateSchedule;
using Application.Common.Handlers.JobSchedules.GetSchedule;
using Application.Common.Handlers.JobSchedules.UpdateSchedule;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.EndPoints;

public record GetJobSchedulesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "systemName")]
    public string[] JobSystemNames { get; init; } = [];

    [FromQuery(Name = "nextRunFrom")]
    public DateTime? NextRunFrom { get; init; }

    [FromQuery(Name = "nextRunTo")]
    public DateTime? NextRunTo { get; init; }
}

public record GetJobSchedulesResponse
{
    [JsonPropertyName("schedules")]
    public required IReadOnlyList<JobScheduleDto> Schedules { get; init; }
}

public record CreateJobScheduleResponse
{
    [JsonPropertyName("schedule")]
    public required JobScheduleDto Schedule { get; init; }
}

public record UpdateJobScheduleResponse
{
    [JsonPropertyName("schedule")]
    public required JobScheduleDto Schedule { get; init; }
}

public record GetJobScheduleResponse(JobScheduleDto Schedule);

public static class JobScheduleEndPoints
{
    public static RouteGroupBuilder AddScheduleEndPoints(this RouteGroupBuilder group)
    {
        var schedules = group.MapGroup("/schedules")
            .WithTags("Schedules");

        schedules.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetJobSchedulesRequest request,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new GetScheduleQuery(
                            request.JobSystemNames,
                            new RangeModel<DateTime>(request.NextRunFrom, request.NextRunTo),
                            request.SortBy,
                            request),
                        ct);

                    return Results.Ok(
                        new GetJobSchedulesResponse
                        {
                            Schedules = result.Schedules
                        });
                })
            .WithName("GetJobSchedules")
            .WithDisplayName("Get job schedules")
            .Produces<GetJobSchedulesResponse>()
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        schedules.MapGet(
                "{scheduleId:guid}",
                async (
                    ISender sender,
                    Guid scheduleId,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new GetScheduleByIdQuery(scheduleId),
                        ct);

                    return Results.Ok(
                        new GetJobScheduleResponse(result.Schedule));
                })
            .WithName("GetJobSchedule")
            .WithDisplayName("Get job schedule by id")
            .Produces<GetJobScheduleResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAllPermissions(PermissionCodes.JOBS_GET);

        schedules.MapPost(
                "",
                async (
                    ISender sender,
                    NewJobScheduleDto schedule,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new CreateScheduleCommand(schedule), ct);
                    var created = await sender.Send(
                        new GetScheduleByIdQuery(result.ScheduleId),
                        ct);

                    return Results.Created(
                        $"/jobs/schedules/{result.ScheduleId}",
                        new CreateJobScheduleResponse
                        {
                            Schedule = created.Schedule
                        });
                })
            .WithName("CreateJobSchedule")
            .WithDisplayName("Create job schedule")
            .Produces<CreateJobScheduleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAllPermissions(PermissionCodes.JOBS_CREATE);

        schedules.MapPatch(
                "{scheduleId:guid}",
                async (
                    ISender sender,
                    Guid scheduleId,
                    PatchJobScheduleDto patch,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new UpdateScheduleCommand(scheduleId, patch), ct);
                    var updated = await sender.Send(
                        new GetScheduleByIdQuery(result.ScheduleId),
                        ct);

                    return Results.Ok(
                        new UpdateJobScheduleResponse
                        {
                            Schedule = updated.Schedule
                        });
                })
            .WithName("UpdateJobSchedule")
            .WithDisplayName("Update job schedule")
            .Produces<UpdateJobScheduleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAllPermissions(PermissionCodes.JOBS_CREATE);

        schedules.MapDelete(
                "{scheduleId:guid}",
                async (
                    ISender sender,
                    Guid scheduleId,
                    CancellationToken ct) =>
                {
                    await sender.Send(new RemoveJobScheduleCommand(scheduleId), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveJobSchedule")
            .WithDisplayName("Remove job schedule")
            .Produces(204)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAllPermissions(PermissionCodes.JOBS_CREATE);

        return group;
    }
}
