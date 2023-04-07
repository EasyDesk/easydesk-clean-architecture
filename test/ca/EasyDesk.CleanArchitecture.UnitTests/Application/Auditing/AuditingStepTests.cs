using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Testing.Unit.Application;
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
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IAuditStorage _auditStorage;
    private readonly FakeClock _clock = new(_now);
    private readonly NextPipelineStep<Nothing> _next;

    public record TestCommandRequest : ICommandRequest<Nothing>;

    public record TestCommand : ICommand;

    public record TestEvent : IEvent;

    public record TestDescriptionOverride : ICommand, IOverrideAuditDescription
    {
        public const string Description = "TestDescription";

        public string GetAuditDescription() => Description;
    }

    public record TestPropertiesOverride : ICommand, IOverrideAuditProperties
    {
        public static readonly IImmutableDictionary<string, string> TestProperties = Map(
            ("A", "B"),
            ("C", "D"));

        public void ConfigureProperties(IDictionary<string, string> properties)
        {
            foreach (var (k, v) in TestProperties)
            {
                properties.Add(k, v);
            }
        }
    }

    public record SkipAuditing : IReadWriteOperation;

    public AuditingStepTests()
    {
        _userInfoProvider = Substitute.For<IUserInfoProvider>();
        _userInfoProvider.UserInfo.Returns(None);

        _auditStorage = Substitute.For<IAuditStorage>();

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);
    }

    private Task<Result<Nothing>> Run<T>() where T : IReadWriteOperation, new() =>
        new AuditingStep<T, Nothing>(_auditStorage, _userInfoProvider, _clock).Run(new T(), _next);

    [Fact]
    public async Task ShouldNotRecordAnyAudit_IfTheRequestIsNotACommandNorAnEvent()
    {
        await Run<SkipAuditing>();

        await _auditStorage.DidNotReceiveWithAnyArgs().StoreAudit(default!);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeCommand(
        Option<string> userId, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestCommand>(AuditRecordType.Command, userId, result);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeCommandRequest(
        Option<string> userId, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestCommandRequest>(AuditRecordType.CommandRequest, userId, result);
    }

    [Theory]
    [MemberData(nameof(AuditData))]
    public async Task ShouldRecordAnAuditOfTypeEvent(
        Option<string> userId, Result<Nothing> result)
    {
        await ShouldRecordAnAudit<TestEvent>(AuditRecordType.Event, userId, result);
    }

    private async Task ShouldRecordAnAudit<T>(AuditRecordType type, Option<string> userId, Result<Nothing> result)
        where T : IReadWriteOperation, new()
    {
        _userInfoProvider.UserInfo.Returns(userId.Map(i => new UserInfo(i)));
        _next().Returns(result);

        var stepResult = await Run<T>();

        await _auditStorage.Received(1).StoreAudit(new(
            Type: type,
            Name: typeof(T).Name,
            Description: None,
            UserId: userId,
            Properties: Map<string, string>(),
            Success: result.IsSuccess,
            Instant: _now));

        stepResult.ShouldBe(result);
    }

    public static IEnumerable<object[]> AuditData()
    {
        return Matrix
            .Axis(Some("userId"), None)
            .Axis(Ok, Failure<Nothing>(TestError.Create()))
            .Build();
    }

    [Fact]
    public async Task ShouldRecordADescriptionIfTheRequestSupportsIt()
    {
        await Run<TestDescriptionOverride>();

        await _auditStorage.Received(1).StoreAudit(Arg.Is<AuditRecord>(r => r.Description.Contains(TestDescriptionOverride.Description)));
    }

    [Fact]
    public async Task ShouldRecordPropertiesIfTheRequestSupportsIt()
    {
        await Run<TestPropertiesOverride>();

        await _auditStorage.Received(1).StoreAudit(Arg.Is<AuditRecord>(r => r.Properties.Equals(TestPropertiesOverride.TestProperties)));
    }
}
