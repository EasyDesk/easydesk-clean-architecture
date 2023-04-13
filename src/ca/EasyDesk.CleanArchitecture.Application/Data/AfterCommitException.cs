namespace EasyDesk.CleanArchitecture.Application.Data;

public sealed class AfterCommitException : Exception
{
    public AfterCommitException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}
