using EasyDesk.Commons.Options;

namespace EasyDesk.Commons.Utils;

public static class Enums
{
    public static Option<T> ParseOption<T>(string s) where T : struct, Enum =>
        TryOption<string, T>(Enum.TryParse, s);
}
