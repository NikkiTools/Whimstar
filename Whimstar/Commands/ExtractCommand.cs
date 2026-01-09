using System.ComponentModel;
using CUE4Parse.Compression;
using Spectre.Console;
using Spectre.Console.Cli;
using Whimstar.Processors;

namespace Whimstar.Commands;

internal class ExtractCommand : AsyncCommand<ExtractCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Path to the input game folder")]
        public required string InputPath { get; init; } = "";

        [CommandOption("-v|--version")]
        [Description("Version to extract data for - defaults to the latest available version, specify 'all' to extract data for all versions")]
        public required string? Version { get; init; } = null;

        [CommandOption("-o|--output")]
        [DefaultValue("output")]
        [Description("Path to the output directory")]
        public required string OutputPath { get; init; } = "";

        [CommandOption("-l|--lua-version")]
        [Description("Version folder name to use for lua extraction - should be in the format <major>_<minor>")]
        public required string LuaVersion { get; init; } = "2_0";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var oodlePath = Path.Join(AppContext.BaseDirectory, OodleHelper.OODLE_DLL_NAME);
        if (!Path.Exists(oodlePath))
        {
            await OodleHelper.DownloadOodleDllAsync(oodlePath);
        }

        OodleHelper.Initialize(oodlePath);

        var provider = new NikkiVfsProvider();

        await provider.InitializeAsync();
        provider.LoadDirectory(settings.InputPath);

        if (settings.Version == "all")
        {
            foreach (var version in provider.UniqueVersions)
            {
                var baseOutput = Path.Join(settings.OutputPath, version.ToString());
                await RunProcessors(provider, version, baseOutput, settings.LuaVersion);
            }
        }
        else
        {
            var version = settings.Version == null
                ? provider.UniqueVersions[^1]
                : NikkiVersion.Parse(settings.Version);

            var baseOutput = Path.Join(settings.OutputPath, version.ToString());
            await RunProcessors(provider, version, baseOutput, settings.LuaVersion);
        }

        return 0;

        static async Task RunProcessors(NikkiVfsProvider provider, NikkiVersion version, string outputPath, string currentVersionName)
        {
            Directory.CreateDirectory(outputPath);

            await provider.ProcessArchivesAsync(version, new ConfigsProcessor(outputPath));
            await provider.ProcessArchivesAsync(version, new LuaTypesProcessor(outputPath, currentVersionName));
            await provider.ProcessArchivesAsync(version, new LocresProcessor(outputPath));
        }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.InputPath))
            return ValidationResult.Error("Input directory does not exist.");

        if (File.Exists(settings.OutputPath))
            return ValidationResult.Error("Output path already exists.");

        if (settings.Version != null && settings.Version != "all" && !NikkiVersion.TryParse(settings.Version, out _))
            return ValidationResult.Error("Invalid version specified.");

        return ValidationResult.Success();
    }
}