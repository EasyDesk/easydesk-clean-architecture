using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public record UnknownAgentError : ApplicationError
{
    public override string GetDetail() => "Missing authentication information on the given request";
}
