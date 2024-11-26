using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.Dto;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[AllowUnknownAgent]
public record TestPolymorphismCommand : ICommandRequest<IPolymorphicDto>
{
    public required IPolymorphicDto PolymorphicDto { get; init; }
}

public class TestPolymorphismHandler : IHandler<TestPolymorphismCommand, IPolymorphicDto>
{
    public Task<Result<IPolymorphicDto>> Handle(TestPolymorphismCommand request)
    {
        return Task.FromResult(Success(request.PolymorphicDto));
    }
}
