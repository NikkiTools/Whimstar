namespace Whimstar.Processors;

public class ConfigsProcessor(string output) : BaseFolderProcessor(Path.Join(output, "config"), "X6Game/Content/config_output");