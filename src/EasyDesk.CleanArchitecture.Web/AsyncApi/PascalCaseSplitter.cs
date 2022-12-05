using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

public static partial class PascalCaseSplitter
{
    [GeneratedRegex("(?!^)([A-Z])")]
    private static partial Regex PascalCaseRegex();

    public static string Split(string pascalString) => PascalCaseRegex().Replace(pascalString, " $1");
}
