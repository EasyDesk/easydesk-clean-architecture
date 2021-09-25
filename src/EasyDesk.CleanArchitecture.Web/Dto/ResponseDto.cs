using EasyDesk.Tools;

namespace EasyDesk.CleanArchitecture.Web.Dto
{
    public record ResponseDto(object Data, object Meta, ErrorDto Error)
    {
        public static ResponseDto Empty(object meta = null) => FromData(Nothing.Value, meta);

        public static ResponseDto FromData(object data, object meta = null) => new(data, meta ?? Nothing.Value, null);

        public static ResponseDto FromError(ErrorDto error, object meta = null) => new(null, meta ?? Nothing.Value, error);
    }
}
