namespace EasyDesk.CleanArchitecture.Web.Csv;

public class InvalidCsvLineException : Exception
{
    public InvalidCsvLineException(string field, string message)
        : base(message)
    {
        Field = field;
    }

    public string Field { get; }
}
