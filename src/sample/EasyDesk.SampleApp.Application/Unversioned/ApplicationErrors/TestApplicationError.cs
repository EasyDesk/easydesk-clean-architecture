using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.SampleApp.Domain.DomainErrors;

namespace EasyDesk.SampleApp.Application.Unversioned.ApplicationErrors;

public record TestApplicationError(string Content) : ApplicationError, IMapFromDomainError<TestApplicationError, TestDomainError>
{
    public override string GetDetail() => "Unversioned test application error";

    public static TestApplicationError MapFrom(TestDomainError src) => new(src.Content);
}
