using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Stubs;

namespace Tests.Tests.Lrt;

public sealed class MultiStepLrtBaseTests
{
    [Fact]
    public async Task ExecuteAsync_FirstRun_QueuesOnlyRootSteps()
    {
        var fixture = new Fixture();

        await fixture.ExecuteAsync();

        fixture.Parent.Status.Should().Be(JobStatus.Waiting);
        fixture.FirstRoot.Status.Should().Be(JobStatus.Pending);
        fixture.SecondRoot.Status.Should().Be(JobStatus.Pending);
        fixture.Child.Status.Should().Be(JobStatus.Blocked);
    }

    [Fact]
    public async Task ExecuteAsync_RootStepsSucceeded_QueuesDependentStep()
    {
        var fixture = new Fixture();
        await fixture.ExecuteAsync();
        fixture.Succeed(fixture.FirstRoot);
        fixture.Succeed(fixture.SecondRoot);
        fixture.ResumeParent();

        await fixture.ExecuteAsync();

        fixture.Parent.Status.Should().Be(JobStatus.Waiting);
        fixture.Child.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_AllStepsSucceeded_SucceedsParent()
    {
        var fixture = new Fixture();
        await fixture.ExecuteAsync();
        fixture.Succeed(fixture.FirstRoot);
        fixture.Succeed(fixture.SecondRoot);
        fixture.ResumeParent();
        await fixture.ExecuteAsync();
        fixture.Succeed(fixture.Child);
        fixture.ResumeParent();

        await fixture.ExecuteAsync();

        fixture.Parent.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_StepFailed_FailsParentWithoutQueuingNextStep()
    {
        var fixture = new Fixture();
        await fixture.ExecuteAsync();
        fixture.Fail(fixture.FirstRoot);
        fixture.ResumeParent();

        await fixture.ExecuteAsync();

        fixture.Parent.Status.Should().Be(JobStatus.Failed);
        fixture.Child.Status.Should().Be(JobStatus.Cancelled);
        fixture.SecondRoot.Status.Should().Be(JobStatus.Cancelled);
    }

    private sealed class Fixture
    {
        private Guid _parentLeaseHolderId = Guid.NewGuid();

        public Fixture()
        {
            UnitOfWork
                .SetupGet(x => x.Context)
                .Returns(new UnitOfWorkContext());
            UnitOfWork
                .Setup(x => x.ExecuteWithTransaction(
                    It.IsAny<TransactionalAttribute>(),
                    It.IsAny<Func<Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns<TransactionalAttribute, Func<Task>, CancellationToken>(
                    (_, action, _) => action());
            UnitOfWork
                .Setup(x => x.ExecuteWithTransaction(
                    It.IsAny<TransactionalAttribute>(),
                    It.IsAny<Func<Task<bool>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns<TransactionalAttribute, Func<Task<bool>>, CancellationToken>(
                    (_, action, _) => action());
            UnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            UnitOfWork
                .Setup(x => x.ReloadAsync(
                    It.IsAny<Job>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Lrt = new TestMultiStepLrt(
                JobRepository.Object,
                UnitOfWork.Object,
                new ApplicationTransactionServiceStub(
                    UnitOfWork.Object,
                    Mock.Of<IRepositoryProvider>()),
                Publisher,
                Logger.Object);

            var registry = new Mock<INamedObjectRegistry<ILrtNamedObject>>();
            registry.Setup(x => x.GetBySystemName(Lrt.SystemName)).Returns(Lrt);
            registry.Setup(x => x.GetBySystemName(It.Is<string>(name =>
                    name == "first-root" || name == "second-root" || name == "child")))
                .Returns((string name) => CreateSingleRunLrt(name));
            Parent = (MultiStepJob)new JobCreationDispatcher(registry.Object)
                .Create(Lrt.SystemName, "{}", 3);
            FirstRoot = Parent.Steps[0];
            SecondRoot = Parent.Steps[1];
            Child = Parent.Steps[2];
            Parent.AcquireLease(_parentLeaseHolderId, TimeSpan.FromMinutes(5));

            JobRepository
                .Setup(x => x.GetById(
                    Parent.Id,
                    It.IsAny<CancellationToken>()))
                .Returns(() => new ValueTask<Job?>(Parent));
            JobRepository
                .Setup(x => x.FirstOrDefaultAsync(
                    It.IsAny<Criteria<Job>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Parent);
            JobRepository
                .Setup(x => x.ListAsync(
                    It.IsAny<Criteria<Job>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Parent.Steps.Cast<Job>().ToList());
        }

        public MultiStepJob Parent { get; }
        public Job FirstRoot { get; }
        public Job SecondRoot { get; }
        public Job Child { get; }
        public TestMultiStepLrt Lrt { get; }
        public Mock<IRepository<Job, Guid>> JobRepository { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public MessageBrokerStub Publisher { get; } = new();
        public Mock<ILogger> Logger { get; } = new();

        public Task ExecuteAsync()
        {
            return Lrt.ExecuteAsync(Parent.Id, _parentLeaseHolderId);
        }

        public void ResumeParent()
        {
            Parent.Resume();
            _parentLeaseHolderId = Guid.NewGuid();
            Parent.AcquireLease(_parentLeaseHolderId, TimeSpan.FromMinutes(5));
        }

        public void Succeed(Job step)
        {
            var leaseHolderId = Guid.NewGuid();
            step.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
            step.Start(leaseHolderId);
            step.Succeed(leaseHolderId);
        }

        public void Fail(Job step)
        {
            var leaseHolderId = Guid.NewGuid();
            step.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
            step.Start(leaseHolderId);
            step.Fail(leaseHolderId, "step failed");
        }
    }

    private sealed class TestMultiStepLrt(
        IRepository<Job, Guid> jobRepository,
        IUnitOfWork unitOfWork,
        IApplicationTransactionService transactionService,
        IPublishEndpoint publisher,
        ILogger logger)
        : MultiStepLrtBase<NoneInputState, NoneInputState>(
            jobRepository,
            unitOfWork,
            publisher,
            transactionService,
            logger)
    {
        public override string SystemName => nameof(TestMultiStepLrt);
        public override string NameLocalizationKey => "test-multi-step-lrt-name";
        public override string DescriptionLocalizationKey =>
            "test-multi-step-lrt-description";
        protected override void ConfigureSteps(
            IMultiStepJobBuilder builder,
            string initialState)
        {
            var firstRoot = builder.AddStep("first-root", initialState);
            var secondRoot = builder.AddStep("second-root", initialState);
            var child = builder.AddStep("child", initialState);
            builder.AddDependency(child, firstRoot);
            builder.AddDependency(child, secondRoot);
        }
    }

    private static ILrtNamedObject CreateSingleRunLrt(string systemName)
    {
        var lrt = new Mock<ILrtNamedObject>();
        lrt.SetupGet(x => x.SystemName).Returns(systemName);
        lrt.SetupGet(x => x.InputType).Returns(typeof(NoneInputState));
        return lrt.Object;
    }

}
