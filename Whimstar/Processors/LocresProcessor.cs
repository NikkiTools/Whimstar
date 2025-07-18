using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar.Processors;

public class LocresProcessor(string output) : IArchiveProcessor
{
    private static readonly string[] LanguagesToExtract =
        ["de", "en", "es", "fr", "id", "it", "ja-JP", "ko", "pt", "th", "zh", "zh-Hant", "zh-SG"];

    private readonly LocresLanguageProcessor[] _languageProcessors =
        LanguagesToExtract.Select(x => new LocresLanguageProcessor(output, x)).ToArray();

    public bool Finished => _languageProcessors.All(x => x.Finished);

    public async Task Process(IAesVfsReader archive, NikkiVersion version)
    {
        foreach (var processor in _languageProcessors.Where(x => !x.Finished))
            await processor.Process(archive, version);
    }

    private class LocresLanguageProcessor(string output, string languageCode)
        : BaseFolderProcessor(Path.Join(output, "localization", "Game", languageCode), $"X6Game/Content/Localization/Game/{languageCode}");
}