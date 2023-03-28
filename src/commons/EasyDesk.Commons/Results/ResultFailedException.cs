namespace EasyDesk.Commons;

public class ResultFailedException : Exception
{
    public ResultFailedException(Error error) : this($"Result failed with error '{error}'", error)
    {
    }

    public ResultFailedException(string message, Error error) : base(message)
    {
        Error = error;
    }

    public Error Error { get; }
}
