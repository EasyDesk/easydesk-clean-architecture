using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Testing.Errors;
using EasyDesk.Testing.MatrixExpansion;
using NodaTime;
using NodaTime.Testing;
using NSubstitute;
using Shouldly;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Auditing;

public class AuditingStepTests
{
    private static readonly Instant _now = Instant.FromUtc(2023, 10, 21, 13, 45);
    private readonly IContextProvider _contextProvider;
    private readonly IAuditStorage _auditStorage;
    private readonly AuditConfigurer _auditConfigurer = new();
    private readonly FakeClock _clock = new(_now);
    private readonly NextPipelineStep<Nothing> _next;

    public record TestCommandRequest : ICommandRequest<Nothing>;

    public record TestCommand : ICommand;

    public record TestEvent : IEvent;

    public record SkipAuditing : IReadWriteOperation;

    public AuditingStepTests()
    {
        _contextProvider = Substitute.For<IContextProvider>();
        _contextProvider.CurrentContext.Returns(new ContextInfo.AnonymousRequest());

        _auditStorage = Substitute.For<IAuditStorage>();

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);
    }

    private Task<Result<Nothing>> Run<T>() where T : IReadWriteOperation, new() =>
        new AuditingStep<T, Nothing>(_auditStorage, _contextProvider, _auditConfigurer, _clock).Run(new T(), _next);

    [Fact]
    public async Task ShouldNotRecordAnyAudit_IfTheRequestIsNotACommandNorAnEvent()
    {
        await Run<SkipAuditing>();

        await _auditStorage.DidNotReceiveWithAnyArgs().StoreAudit(default!);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeCommand(
        Option<Agent> agent, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestCommand>(AuditRecordType.Command, agent, result);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeCommandRequest(
        Option<Agent> agent, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestCommandRequest>(AuditRecordType.CommandRequest, agent, result);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeEvent(
        Option<Agent> agent, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestEvent>(AuditRecordType.Event, agent, result);
    }

    private async Task ShouldRecordAnAudit<T>(AuditRecordType type, Option<Agent> agent, Result<Nothing> result)
        where T : IReadWriteOperation, new()
    {
        _contextProvider.CurrentContext.Returns(agent.Match<ContextInfo>(
            some: i => new ContextInfo.AuthenticatedRequest(i),
            none: () => new ContextInfo.AnonymousRequest()));
        _next().Returns(result);

        var stepResult = await Run<T>();

        await _auditStorage.Received(1).StoreAudit(new(
            Type: type,
            Name: typeof(T).Name,
            Description: None,
            Agent: agent,
            Properties: Map<string, IFixedSet<string>>(),
            Success: result.IsSuccess,
            Instant: _now));

        stepResult.ShouldBe(result);
    }

    public static IEnumerable<object?[]> AuditData()
    {
        var identityId = new IdentityId("identity-id");
        return Matrix.Builder()
            .OptionAxis(
                Agent.Construct(x => x.AddIdentity(Realm.Default, identityId)),
                Agent.Construct(x => x
                    .AddIdentity(Realm.Default, identityId)
                    .AddAttribute("name", "john")
                    .AddAttribute("role", "admin")
                    .AddAttribute("role", "user")))
            .ResultAxis<Nothing>(builder => builder.Failure(TestError.Create()))
            .Build();
    }

    [Fact]
    public async Task ShouldRecordADescriptionIfTheRequestSupportsIt()
    {
        var description = "Test description";
        _auditConfigurer.SetDescription(description);

        await Run<TestCommand>();

        await _auditStorage.Received(1).StoreAudit(Arg.Is<AuditRecord>(r => r.Description.Contains(description)));
    }

    [Fact]
    public async Task ShouldRecordPropertiesIfTheRequestSupportsIt()
    {
        _auditConfigurer.AddProperty("A", "B");
        _auditConfigurer.AddProperty("C", "D");

        await Run<TestCommand>();

        await _auditStorage.Received(1).StoreAudit(Arg.Is<AuditRecord>(r => r.Properties.Equals(_auditConfigurer.Properties)));
    }
}
