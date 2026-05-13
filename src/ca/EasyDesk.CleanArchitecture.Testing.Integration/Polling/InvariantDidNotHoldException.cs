namespace EasyDesk.CleanArchitecture.Testing.Integration.Polling;

public class InvariantDidNotHoldException : Exception
{
    public InvariantDidNotHoldException(Exception? innerException = null)
        : base("The invariant did not hold at some point during the polling.", innerException)
    {
    }
}
