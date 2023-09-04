using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.SampleApp.Domain.DomainErrors;

namespace EasyDesk.SampleApp.Application.V_1_5.ApplicationErrors;

public record TestApplicationError(string Content) : ApplicationError, IMapFromDomainError<TestApplicationError, TestDomainError>
{
    public override string GetDetail() => "Test application error v1.5";

    public static TestApplicationError MapFrom(TestDomainError src) => new(src.Content);
}
