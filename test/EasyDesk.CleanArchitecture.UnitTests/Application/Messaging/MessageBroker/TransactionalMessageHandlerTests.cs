using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Testing;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;

public class TransactionalMessageHandlerTests
{
    private readonly TransactionalMessageHandler _sut;
    private readonly IMessageHandler _handler;
    private readonly ITransactionManager _transactionManager;
    private readonly Message _message = Message.CreateEmpty();

    public TransactionalMessageHandlerTests()
    {
        _handler = Substitute.For<IMessageHandler>();
        _handler.Handle(_message).Returns(MessageHandlerResult.Handled);

        _transactionManager = Substitute.For<ITransactionManager>();
        _transactionManager.Commit().Returns(Ok);

        _sut = new(_handler, _transactionManager);
    }

    [Theory]
    [InlineData(MessageHandlerResult.Handled)]
    [InlineData(MessageHandlerResult.GenericFailure)]
    [InlineData(MessageHandlerResult.NotSupported)]
    [InlineData(MessageHandlerResult.TransientFailure)]
    public async Task Handle_ShouldReturnTheInnerHandlersResult_IfTheCommitSucceedes(
        MessageHandlerResult expectedResult)
    {
        _handler.Handle(_message).Returns(expectedResult);

        var actualResult = await _sut.Handle(_message);

        actualResult.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldWrapTheInnerHandlerInTheUnitOfWork()
    {
        await _sut.Handle(_message);

        Received.InOrder(() =>
        {
            _transactionManager.Begin();
            _handler.Handle(_message);
            _transactionManager.Commit();
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnTransientFailure_IfTheCommitFails()
    {
        _transactionManager.Commit().Returns(TestError.Create());

        var result = await _sut.Handle(_message);

        result.ShouldBe(MessageHandlerResult.TransientFailure);
    }
}
