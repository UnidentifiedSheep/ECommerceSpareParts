using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Tests.Lrt;

public sealed class JobCreationDispatcherTests
{
    [Fact]
    public void Create_SingleRunLrt_CreatesSingleRunJobWithValidatedState()
    {
        var lrt = new Mock<ILrtNamedObject>();
        lrt.SetupGet(x => x.SystemName).Returns("single-run");
        lrt.SetupGet(x => x.InputType).Returns(typeof(NoneInputState));
        var dispatcher = CreateDispatcher("single-run", lrt.Object);

        var job = dispatcher.Create("single-run", "{}", 5);

        job.Should().BeOfType<SingleRunJob>();
        job.Status.Should().Be(JobStatus.Pending);
        job.SystemName.Should().Be("single-run");
        job.MaxAttempts.Should().Be(5);
        job.State.Should().Be("{}");
    }

    [Fact]
    public void Create_MultiStepLrt_CreatesTopologyWithBlockedSteps()
    {
        var lrt = new TestMultiStepLrt();
        var stepLrt = CreateSingleRunLrt("step");
        var dispatcher = CreateDispatcher(lrt, stepLrt);

        var job = dispatcher.Create(lrt.SystemName, "{}", 4);

        var multiStepJob = job.Should().BeOfType<MultiStepJob>().Subject;
        multiStepJob.MaxAttempts.Should().Be(4);
        multiStepJob.Steps.Should().ContainSingle()
            .Which.Status.Should().Be(JobStatus.Blocked);
    }

    [Fact]
    public void Create_NestedMultiStepLrt_CreatesNestedBlockedTopology()
    {
        var outer = new NestedMultiStepLrt("outer", "inner");
        var inner = new NestedMultiStepLrt("inner", "leaf");
        var leaf = CreateSingleRunLrt("leaf");
        var dispatcher = CreateDispatcher(outer, inner, leaf);

        var job = dispatcher.Create(outer.SystemName, "{}", 3);

        var outerJob = job.Should().BeOfType<MultiStepJob>().Subject;
        var innerJob = outerJob.Steps.Should().ContainSingle()
            .Which.Should().BeOfType<MultiStepJob>().Subject;
        innerJob.Status.Should().Be(JobStatus.Blocked);
        innerJob.Steps.Should().ContainSingle()
            .Which.Should().BeOfType<SingleRunJob>()
            .Which.Status.Should().Be(JobStatus.Blocked);
    }

    [Fact]
    public void Create_CyclicMultiStepComposition_Throws()
    {
        var first = new NestedMultiStepLrt("first", "second");
        var second = new NestedMultiStepLrt("second", "first");
        var dispatcher = CreateDispatcher(first, second);

        var act = () => dispatcher.Create(first.SystemName, "{}", 3);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cycle*first*");
    }

    private static JobCreationDispatcher CreateDispatcher(
        string systemName,
        ILrtNamedObject lrt)
    {
        var registry = new Mock<INamedObjectRegistry<ILrtNamedObject>>();
        registry.Setup(x => x.GetBySystemName(systemName)).Returns(lrt);
        return new JobCreationDispatcher(registry.Object);
    }

    private static JobCreationDispatcher CreateDispatcher(
        params ILrtNamedObject[] lrts)
    {
        var registry = new Mock<INamedObjectRegistry<ILrtNamedObject>>();
        registry.Setup(x => x.GetBySystemName(It.IsAny<string>()))
            .Returns((string name) => lrts.Single(x => x.SystemName == name));
        return new JobCreationDispatcher(registry.Object);
    }

    private static ILrtNamedObject CreateSingleRunLrt(string systemName)
    {
        var lrt = new Mock<ILrtNamedObject>();
        lrt.SetupGet(x => x.SystemName).Returns(systemName);
        lrt.SetupGet(x => x.InputType).Returns(typeof(NoneInputState));
        return lrt.Object;
    }

    private sealed class TestMultiStepLrt : MultiStepLrtBase<NoneInputState, NoneInputState>
    {
        public TestMultiStepLrt()
            : base(
                Mock.Of<IRepository<Job, Guid>>(),
                Mock.Of<IUnitOfWork>(),
                Mock.Of<IPublishEndpoint>(),
                Mock.Of<ILogger>())
        {
        }

        public override IServiceDefinition ServiceDefinition { get; } = new TestServiceDefinition();
        public override string SystemName => nameof(TestMultiStepLrt);
        public override string NameLocalizationKey => "test-name";
        public override string DescriptionLocalizationKey => "test-description";
        protected override void ConfigureSteps(
            IMultiStepJobBuilder builder,
            string initialState)
        {
            builder.AddStep("step", initialState);
        }
    }

    private sealed class NestedMultiStepLrt(
        string systemName,
        string childSystemName) : MultiStepLrtBase<NoneInputState, NoneInputState>(
        Mock.Of<IRepository<Job, Guid>>(),
        Mock.Of<IUnitOfWork>(),
        Mock.Of<IPublishEndpoint>(),
        Mock.Of<ILogger>())
    {
        public override IServiceDefinition ServiceDefinition { get; } =
            new TestServiceDefinition();
        public override string SystemName => systemName;
        public override string NameLocalizationKey => "test-name";
        public override string DescriptionLocalizationKey => "test-description";
        protected override void ConfigureSteps(
            IMultiStepJobBuilder builder,
            string initialState)
        {
            builder.AddStep(childSystemName, initialState);
        }
    }

    private sealed class TestServiceDefinition : IServiceDefinition
    {
        public string ServiceName => "test";
    }
}
