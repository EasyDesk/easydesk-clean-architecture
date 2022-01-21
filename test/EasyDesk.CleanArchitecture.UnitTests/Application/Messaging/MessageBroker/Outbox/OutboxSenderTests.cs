using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender.Outbox;
using EasyDesk.CleanArchitecture.Testing;
using EasyDesk.Testing;
using EasyDesk.Tools.Observables;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker.MessageBrokerTestingUtils;
using static EasyDesk.Tools.Collections.EnumerableUtils;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker.Outbox;

public class OutboxSenderTests
{
    private readonly OutboxSender _sut;
    private readonly ITransactionManager _transactionManager;
    private readonly IOutbox _outbox;
    private readonly IOutboxChannel _outboxChannel;
    private readonly SimpleAsyncEvent<AfterCommitContext> _afterCommit = new();

    public OutboxSenderTests()
    {
        _transactionManager = Substitute.For<ITransactionManager>();
        _transactionManager.AfterCommit.Returns(_afterCommit);

        _outbox = Substitute.For<IOutbox>();

        _outboxChannel = Substitute.For<IOutboxChannel>();

        _sut = new(_transactionManager, _outbox, _outboxChannel);
    }

    [Fact]
    public async Task Send_ShouldStoreMessagesInTheOutbox()
    {
        var messages = NewMessageSequence(2);

        await _sut.Send(messages);

        await _outbox.Received(1).StoreMessages(Args.Are(messages));
    }

    [Fact]
    public async Task Send_ShouldNotifyTheOutboxChannelOfAllMessageGroups_WhenUnitOfWorkIsCommitted()
    {
        var messageGroups = Items(NewMessageSequence(2), NewMessageSequence(3));

        foreach (var group in messageGroups)
        {
            await _sut.Send(group);
        }
        await Commit();

        Received.InOrder(() =>
        {
            foreach (var group in messageGroups)
            {
                _outboxChannel.OnNewMessageGroup(Args.Are(group.Select(x => x.Id)));
            }
        });
    }

    [Fact]
    public async Task Send_ShouldNotNotifyTheOutbxChannel_IfCommitFails()
    {
        await _sut.Send(NewMessageSequence(2));
        await Commit(successful: false);

        _outboxChannel.DidNotReceiveWithAnyArgs().OnNewMessageGroup(default);
    }

    private async Task Commit(bool successful = true)
    {
        var context = new AfterCommitContext(successful ? None : Some(TestError.Create()));
        await _afterCommit.Emit(context);
    }
}
