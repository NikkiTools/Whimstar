using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar;

public class NikkiVfsProvider() : AbstractVfsFileProvider(Version)
{
    private static readonly VersionContainer Version = new(EGame.GAME_InfinityNikki);

    private readonly Dictionary<NikkiVersion, HashSet<IAesVfsReader>> _packagesByVersion = [];
    private static readonly string[] ArchiveExtensions = [".pak", ".utoc"];

    private AesKeyInfo? _cachedAesKeyInfo;
    private readonly HashSet<string> _processedArchives = [];
    private readonly List<NikkiVersion> _sortedVersions = [];

    public IReadOnlyList<NikkiVersion> UniqueVersions => _sortedVersions.AsReadOnly();

    public override void Initialize() { }

    public async Task InitializeAsync()
    {
        _cachedAesKeyInfo ??= await ApiUtilities.GetAesKeyInfo();
    }

    public void LoadDirectory(string baseDirectory)
    {
        foreach (var file in Directory.EnumerateFiles(baseDirectory, "*", SearchOption.AllDirectories))
        {
            var extension = Path.GetExtension(file).ToLower();
            if (ArchiveExtensions.Contains(extension))
            {
                RegisterVfs(file);
            }
        }

        UpdateAesKeys();
        UpdateVfs();
    }

    public async Task ProcessArchivesAsync(NikkiVersion maxVersion, IArchiveProcessor processor)
    {
        // this takes all versions equal to or below the max version and iterates through them,
        // starting at the highest version
        foreach (var version in _sortedVersions.Take(_sortedVersions.IndexOf(maxVersion) + 1).Reverse())
        {
            foreach (var archive in _packagesByVersion[version])
            {
                if (processor.Finished)
                    return;

                await processor.Process(archive, version);
            }
        }
    }

    private void UpdateAesKeys()
    {
        if (_cachedAesKeyInfo != null)
        {
            SubmitKey(new FGuid(), new FAesKey(_cachedAesKeyInfo.MainKey));
            SubmitKeys(_cachedAesKeyInfo.DynamicKeys.Select(x =>
                new KeyValuePair<FGuid, FAesKey>(new FGuid(x.Guid), new FAesKey(x.Key))));
        }
    }

    private void UpdateVfs()
    {
        foreach (var vfs in MountedVfs)
        {
            if (!_processedArchives.Add(vfs.Name))
                continue;

            var name = ArchiveName.Parse(vfs.Name);

            _packagesByVersion.TryAdd(name.PakVersion, []);
            _packagesByVersion[name.PakVersion].Add(vfs);

            if (!_sortedVersions.Contains(name.PakVersion))
                _sortedVersions.Add(name.PakVersion);
        }

        _sortedVersions.Sort();
    }
}