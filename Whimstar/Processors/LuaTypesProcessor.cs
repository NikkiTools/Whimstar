namespace Whimstar.Processors;

public class LuaTypesProcessor(string output) : BaseFolderProcessor(Path.Join(output, "lua"), "X6Game/Content/Script/GenV2");