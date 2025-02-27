namespace EasyDesk.CleanArchitecture.Web.Csv;

public class MissingCsvValueException : Exception
{
    public string Field { get; }

    public MissingCsvValueException(string field) : base($"value is missing.")
    {
        Field = field;
    }
}
