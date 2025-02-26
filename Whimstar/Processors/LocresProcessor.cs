namespace Whimstar.Processors;

public class LocresProcessor(string output) : BaseFolderProcessor(Path.Join(output, "localization"), "X6Game/Content/Localization");