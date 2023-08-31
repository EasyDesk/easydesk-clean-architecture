using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public abstract record ApplicationError : Error
{
    public abstract string GetDetail();
}
