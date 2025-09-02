using System.Security.Cryptography;
using System.Text;
using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar.Processors;

// Pre v1.9:
// public class LuaTypesProcessor(string output) : BaseFolderProcessor(Path.Join(output, "lua"), "X6Game/Content/Script/GenV2");

public class LuaTypesProcessor(string output) : IArchiveProcessor
{
    public bool Finished => _processor.Finished;

    private readonly ScriptProcessor _processor = new(Path.Join(output, "lua"));

    // TODO: This should be configurable
    private const string CurrentTypesVersion = "1_9";

    private static string TranslateLuaPath(string actualPath)
    {
        // lowercase relative path with .lua extension
        var hashedFilename = Convert.ToHexString(
            SHA1.HashData(
            Encoding.UTF8.GetBytes(
                actualPath
                    .Replace('\\', '/')
                    .ToLowerInvariant())));

        return $"X6Game/Content/Script/{hashedFilename}";
    }

    public async Task Process(IAesVfsReader archive, NikkiVersion version)
    {
        var hashedPath = TranslateLuaPath($"GenV2/{CurrentTypesVersion}/Cfg/CfgTypes.lua");
        if (!archive.Files.ContainsKey(hashedPath))
            return;

        await _processor.Process(archive, version);
    }

    private class ScriptProcessor(string output) : BaseFolderProcessor(output, "X6Game/Content/Script");
}