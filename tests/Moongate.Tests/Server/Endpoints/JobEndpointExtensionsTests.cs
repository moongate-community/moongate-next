using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Abstractions.Data.Jobs;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Abstractions.Types.Jobs;
using Moongate.Server.Extensions.Endpoints;

namespace Moongate.Tests.Server.Endpoints;

public sealed class JobEndpointExtensionsTests
{
    private sealed class FakeJobService : IJobService
    {
        public List<JobSnapshot> Jobs { get; } = new();

        public string? RanJobId { get; private set; }

        public bool RunResult { get; set; } = true;

        public bool Cancel(string jobId)
            => true;

        public IReadOnlyList<JobSnapshot> GetJobs()
            => Jobs;

        public string RegisterOnce(
            string name,
            TimeSpan delay,
            Action handler,
            string? description = null,
            JobSourceType source = JobSourceType.CSharp
        )
            => "id";

        public string RegisterRecurring(
            string name,
            TimeSpan interval,
            Action handler,
            string? description = null,
            bool runImmediately = false,
            JobSourceType source = JobSourceType.CSharp
        )
            => "id";

        public bool RunNow(string jobId)
        {
            RanJobId = jobId;

            return RunResult;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void HandleList_ReturnsJobs()
    {
        var svc = new FakeJobService();
        svc.Jobs.Add(
            new(
                "id",
                "save",
                null,
                JobSourceType.CSharp,
                1000,
                true,
                null,
                null,
                null,
                JobStatusType.NeverRun,
                null,
                0
            )
        );

        var ok = Assert.IsType<Ok<IReadOnlyList<JobSnapshot>>>(JobEndpointExtensions.HandleList(svc));

        Assert.Single(ok.Value!);
    }

    [Fact]
    public void HandleRun_KnownJob_ReturnsOk()
    {
        var svc = new FakeJobService { RunResult = true };

        Assert.IsType<Ok>(JobEndpointExtensions.HandleRun(svc, "abc"));
        Assert.Equal("abc", svc.RanJobId);
    }

    [Fact]
    public void HandleRun_UnknownJob_ReturnsNotFound()
        => Assert.IsType<NotFound>(JobEndpointExtensions.HandleRun(new FakeJobService { RunResult = false }, "missing"));
}
