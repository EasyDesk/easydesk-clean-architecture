using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Application.CommandLine;

public record CliContext
{
    public required ParseResult ParseResult { get; init; }

    public required CancellationToken CancellationToken { get; init; }
}
