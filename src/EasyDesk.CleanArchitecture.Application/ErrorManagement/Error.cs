namespace EasyDesk.CleanArchitecture.Application.ErrorManagement
{
    public abstract record Error(string Message, string ErrorCode)
    {
    }
}
