namespace EasyDesk.CleanArchitecture.Application.Data;

public class AfterCommitException : Exception
{
    public AfterCommitException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}
