using Moongate.Abstractions.Data.Jobs;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Core.Types;

namespace Moongate.Server.Extensions.Endpoints;

public static class JobEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateJobs(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/jobs")
            .WithTags("Admin Jobs")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet("/", (IJobService jobs) => HandleList(jobs))
            .WithName("ListAdminJobs")
            .WithSummary("Returns the registered jobs and their latest run metadata.")
            .Produces<IReadOnlyList<JobSnapshot>>();

        group.MapPost("/{id}/run", (IJobService jobs, string id) => HandleRun(jobs, id))
            .WithName("RunAdminJob")
            .WithSummary("Schedules an immediate run of a job.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    internal static IResult HandleList(IJobService jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return TypedResults.Ok(jobs.GetJobs());
    }

    internal static IResult HandleRun(IJobService jobs, string id)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.RunNow(id) ? TypedResults.Ok() : TypedResults.NotFound();
    }
}
