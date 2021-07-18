using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.Testing.Utils
{
    public record TestError(string Code) : Error(ErrorCode: Code, Message: "MESSAGE")
    {
        public static Error Create() => Create("ERROR_CODE");

        public static Error Create(string code) => new TestError(code);
    }
}
