using EasyDesk.Tools;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto(object Data, object Meta, IEnumerable<ErrorDto> Errors)
{
    public static ResponseDto Empty(object meta = null) => FromData(Nothing.Value, meta);

    public static ResponseDto FromData(object data, object meta = null) => new(data, meta ?? Nothing.Value, Enumerable.Empty<ErrorDto>());

    public static ResponseDto FromErrors(IEnumerable<ErrorDto> errors, object meta = null) => new(null, meta ?? Nothing.Value, errors);

    public static ResponseDto FromError(ErrorDto error, object meta = null) => FromErrors(Some(error), meta);
}
