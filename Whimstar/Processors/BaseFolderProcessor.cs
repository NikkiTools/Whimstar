using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar.Processors;

public abstract class BaseFolderProcessor(string outputDirectory, string archiveFolderPath) : IArchiveProcessor
{
    public bool Finished { get; private set; }

    public async Task Process(IAesVfsReader archive, NikkiVersion version)
    {
        var extractedAnyFile = false;

        foreach (var file in archive.Files.Values.Where(x => x.Path.StartsWith(archiveFolderPath)))
        {
            if (!extractedAnyFile)
            {
                Console.WriteLine($"Extracting data from {archiveFolderPath} in {archive.Name} - Archive version {version}");
                extractedAnyFile = true;
            }

            var fileData = await file.ReadAsync();
            var outputPath = Path.Join(outputDirectory, file.Path);

            var parent = Directory.GetParent(outputPath);
            if (parent is { Exists: false })
                parent.Create();

            await File.WriteAllBytesAsync(outputPath, fileData);
        }

        Finished = extractedAnyFile;
    }
}