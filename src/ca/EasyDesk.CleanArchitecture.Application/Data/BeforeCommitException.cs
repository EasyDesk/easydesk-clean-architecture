namespace EasyDesk.CleanArchitecture.Application.Data;

public sealed class BeforeCommitException : Exception
{
    public BeforeCommitException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}
