// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using CUE4Parse.Compression;
using Spectre.Console;
using Spectre.Console.Cli;
using Whimstar;
using Whimstar.Processors;

var app = new CommandApp<ExtractCommand>();
await app.RunAsync(args);

namespace Whimstar
{
    class ExtractCommand : AsyncCommand<ExtractCommand.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<input>")]
            [Description("Path to the input game folder")]
            public required string InputPath { get; init; } = "";

            [CommandOption("-v|--version")]
            [Description("Version to extract data for. Default the latest available version. 'all' extracts all data")]
            public required string? Version { get; init; } = null;

            [CommandOption("-o|--output")]
            [DefaultValue("output")]
            [Description("Path to the output directory")]
            public required string OutputPath { get; init; } = "";
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            OodleHelper.DownloadOodleDll();
            OodleHelper.Initialize(OodleHelper.OODLE_DLL_NAME);

            var provider = new NikkiVfsProvider();

            await provider.InitializeAsync();
            provider.LoadDirectory(settings.InputPath);

            if (settings.Version == "all")
            {
                foreach (var version in provider.UniqueVersions)
                {
                    var baseOutput = Path.Join(settings.OutputPath, version.ToString());
                    Directory.CreateDirectory(baseOutput);

                    await provider.ProcessArchivesAsync(version, new ConfigsProcessor(baseOutput));
                    await provider.ProcessArchivesAsync(version, new LuaTypesProcessor(baseOutput));
                }
            }
            else
            {
                var version = settings.Version == null
                    ? provider.UniqueVersions[^1]
                    : NikkiVersion.Parse(settings.Version);

                var baseOutput = Path.Join(settings.OutputPath, version.ToString());
                Directory.CreateDirectory(baseOutput);

                await provider.ProcessArchivesAsync(version, new ConfigsProcessor(baseOutput));
                await provider.ProcessArchivesAsync(version, new LuaTypesProcessor(baseOutput));
            }

            return 0;
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
}