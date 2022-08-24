namespace EasyDesk.CleanArchitecture.Application.Data;

public class BeforeCommitException : Exception
{
    public BeforeCommitException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}
