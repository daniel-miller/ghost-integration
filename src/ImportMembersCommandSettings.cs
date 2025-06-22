using Spectre.Console.Cli;

namespace GhostIntegration;

internal sealed partial class ImportMembersCommandSettings : CommandSettings
{
    [CommandOption("--input")]
    public string? Input { get; init; }

    [CommandOption("--key")]
    public string? Key { get; init; }

    [CommandOption("--url")]
    public string? Url { get; init; }
}
