using CUE4Parse.UE4.VirtualFileSystem;

namespace Whimstar;

public interface IArchiveProcessor
{
    bool Finished { get; }

    Task Process(IAesVfsReader archive, NikkiVersion version);
}