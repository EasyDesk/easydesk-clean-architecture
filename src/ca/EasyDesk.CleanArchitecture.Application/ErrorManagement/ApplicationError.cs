using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public abstract record ApplicationError : Error
{
    public abstract string GetDetail();

    public override string ToString()
    {
        return $"{base.ToString()} ({GetDetail()})";
    }
}
