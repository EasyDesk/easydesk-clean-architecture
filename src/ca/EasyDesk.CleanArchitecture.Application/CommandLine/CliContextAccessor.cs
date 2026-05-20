using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.CommandLine;

public class CliContextAccessor
{
    public Option<CliContext> CliContext { get; internal set; }
}
