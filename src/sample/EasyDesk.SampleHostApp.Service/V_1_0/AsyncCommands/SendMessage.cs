using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleHostApp.Service.V_1_0.AsyncCommands;

public record SendMessage : IIncomingCommand
{
    public required string Message { get; init; }
}

public record ReceiveMessage : IOutgoingCommand
{
    public const string RecipientQueue = "recipient";

    public required string Message { get; init; }

    public static string GetDestination(RoutingContext context) => RecipientQueue;
}

public class SendMessageHandler : IHandler<SendMessage>
{
    private readonly ICommandSender _commandSender;

    public SendMessageHandler(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    public async Task<Result<Nothing>> Handle(SendMessage request)
    {
        await _commandSender.Send(new ReceiveMessage
        {
            Message = request.Message,
        });

        return Ok;
    }
}
