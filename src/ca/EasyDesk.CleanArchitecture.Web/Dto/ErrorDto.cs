using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Strings;
using System.Text;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ErrorDto
{
    public required string Code { get; init; }

    public required string Detail { get; init; }

    public required object Meta { get; init; }

    public static IEnumerable<ErrorDto> CreateErrorDtoList(Error error) => error switch
    {
        MultiError(var errors) => errors.Select(FromError),
        _ => Some(FromError(error)),
    };

    public static ErrorDto FromError(Error error) => error switch
    {
        ApplicationError e => new()
        {
            Code = GetErrorCodeFromApplicationErrorType(e.GetType()),
            Detail = e.GetDetail(),
            Meta = e,
        },
        DomainError => new()
        {
            Code = GetErrorCodeFromDomainErrorType(error.GetType()),
            Detail = ConvertPascalCaseToHumanReadable(error.GetType().Name),
            Meta = error,
        },
        _ => new()
        {
            Code = "Internal",
            Detail = "Unknown internal error occurred",
            Meta = Nothing.Value,
        },
    };

    private static string ConvertPascalCaseToHumanReadable(string pascalCaseText)
    {
        if (pascalCaseText.Length < 2)
        {
            return pascalCaseText;
        }
        var stringBuilder = new StringBuilder();
        var charArray = pascalCaseText.ToCharArray();
        stringBuilder.Append(charArray[0]);
        for (var i = 1; i < charArray.Length - 1; i++)
        {
            var c = charArray[i];
            if (char.IsUpper(c) && (char.IsLower(charArray[i + 1]) || char.IsLower(charArray[i - 1])))
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(char.ToLower(c));
        }
        stringBuilder.Append(charArray[^1]);
        return stringBuilder.ToString();
    }

    public static string GetErrorCodeFromApplicationErrorType(Type errorType) =>
        errorType.Name.RemoveSuffix("Dto").RemoveSuffix("Error");

    public static string GetErrorCodeFromDomainErrorType(Type domainErrorType) =>
        $"DomainError.{domainErrorType.Name}";
}
