using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar.Processors;

public class ConfigsProcessor(string output) : IArchiveProcessor
{
    private const string BaseConfigPath = "X6Game/Content/config_output";
    private const string TableExtension = ".bin";
    private const string BinExtraExtension = ".binextra";

    private readonly string _outputFolder = Path.Join(output, "config");

    public bool Finished => _tableNames.Count == _binExtraNames.Count && _extractedAnyFile;

    private bool _extractedAnyFile;

    private readonly HashSet<string> _tableNames = [];
    private readonly HashSet<string> _binExtraNames = [];

    public async Task Process(IAesVfsReader archive, NikkiVersion version)
    {
        if (!_extractedAnyFile)
        {
            foreach (var file in archive.Files.Values.Where(x => x.Path.StartsWith(BaseConfigPath)))
            {
                if (!_extractedAnyFile)
                {
                    Console.WriteLine(
                        $"Extracting data from {BaseConfigPath} in {archive.Name} - Archive version {version}");
                    _extractedAnyFile = true;
                }

                await ExtractFile(file);

                switch (Path.GetExtension(file.Name))
                {
                    case TableExtension:
                        _tableNames.Add(file.NameWithoutExtension);
                        break;
                    case BinExtraExtension:
                        _binExtraNames.Add(file.NameWithoutExtension);
                        break;
                }
            }
        }
        else
        {
            foreach (var file in archive.Files.Values.Where(x => x.Path.StartsWith(BaseConfigPath)))
            {
                var fileName = file.NameWithoutExtension;
                switch (Path.GetExtension(file.Name))
                {
                    case TableExtension:
                        if (_tableNames.Add(fileName))
                        {
                            Console.WriteLine(
                                $"Extracting {file.Name} in {archive.Name} - Archive version {version}");

                            await ExtractFile(file);
                        }

                        break;
                    case BinExtraExtension:
                        if (_binExtraNames.Add(fileName))
                        {
                            Console.WriteLine(
                                $"Extracting {file.Name} in {archive.Name} - Archive version {version}");

                            await ExtractFile(file);
                        }

                        break;
                }
            }
        }
    }

    private async ValueTask ExtractFile(GameFile file)
    {
        var fileData = await file.ReadAsync();
        var outputPath = Path.Join(_outputFolder, file.Path);

        var parent = Directory.GetParent(outputPath);
        if (parent is { Exists: false })
            parent.Create();

        await File.WriteAllBytesAsync(outputPath, fileData);
    }
}