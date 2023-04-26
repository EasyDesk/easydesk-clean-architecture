using EasyDesk.CleanArchitecture.Application.Cqrs.Async;

namespace EasyDesk.SampleApp.Application.OutgoingEvents;

public record CancellationFailed : IOutgoingEvent;
