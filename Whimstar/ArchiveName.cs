using System.Diagnostics.CodeAnalysis;

namespace Whimstar;

public record ArchiveName(
    string Group = "", // Base, Default, Delay
    string Name = "", 
    NikkiVersion PakVersion = default, // 1.0 -> 338, 1.1 -> 372, etc.
    NikkiVersion BaseVersion = default // used for archives when updating between two versions, i.e. 1.0 -> 1.1 (338 -> 372) or 1.1 -> 1.2 (372 -> 387)
)
{
    private const string PatchPakPrefix = "PatchPak";

    public static readonly ArchiveName Global = new("", "global");

    public static ArchiveName Parse(string fileName)
    {
        if (!TryParse(fileName, out var name))
            throw new InvalidOperationException($"Failed to parse archive filename {fileName}");

        return name;
    }

    public static bool TryParse(string fileName, [NotNullWhen(true)] out ArchiveName? name)
    {
        name = null;

        var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
        if (parts is ["global"])
        {
            // special case-ing global.ucas/utoc here so all files can be parsed
            name = Global;
            return true;
        }
        
        // All files (except for global.ucas/utoc) are patch pak files, so we can check for it here.
        if (parts.Length == 0 || parts[^1] != "P")
            return false;

        // group - name - version - 'P'
        if (4 > parts.Length)
            return false;

        var group = parts[0];
        if (group.StartsWith(PatchPakPrefix))
        {
            // this is a hotfix pak filename - we need to parse these differently
            var pakName = parts[0];

            var majorVersion = int.Parse(parts[^2]);
            var hotfixVersion = int.Parse(parts[^3]);
            var version = new NikkiVersion(majorVersion, hotfixVersion);
            if (parts.Length > 4)
            {
                // this is a patch pak part of another group (only seen for paks containing movies)
                // for now we *disregard* everything but the group from that internal name
                group = parts[1];
            }

            name = new ArchiveName(group, pakName, version, default);
        }
        else
        {
            var version = NikkiVersion.Parse(parts[^2]);
            var baseVersion = default(NikkiVersion);

            var nameEndOffset = 2;
            if (parts[^4].Length == 2 && parts[^4].StartsWith('m'))
            {
                // this is an archive used for patching between different major versions
                baseVersion = NikkiVersion.Parse(parts[^3]);
                nameEndOffset = 4;
            }

            var pakName = string.Join("_", parts[1..^nameEndOffset]);
            name = new ArchiveName(group, pakName, version, baseVersion);
        }

        return true;
    }
}