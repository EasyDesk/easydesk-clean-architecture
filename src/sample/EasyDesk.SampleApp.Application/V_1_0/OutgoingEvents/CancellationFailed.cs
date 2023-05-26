using EasyDesk.CleanArchitecture.Application.Cqrs.Async;

namespace EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;

public record CancellationFailed : IOutgoingEvent;
