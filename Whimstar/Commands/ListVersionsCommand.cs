using CUE4Parse.Compression;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Whimstar.Commands;

internal class ListVersionsCommand : AsyncCommand<ListVersionsCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Path to the input game folder")]
        public required string InputPath { get; init; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        OodleHelper.DownloadOodleDll();
        OodleHelper.Initialize(OodleHelper.OODLE_DLL_NAME);

        var provider = new NikkiVfsProvider();

        await provider.InitializeAsync();
        provider.LoadDirectory(settings.InputPath);

        Console.WriteLine("Available versions:");
        foreach (var version in provider.UniqueVersions)
            Console.WriteLine(version);

        return 0;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.InputPath))
            return ValidationResult.Error("Input directory does not exist.");

        return ValidationResult.Success();
    }
}