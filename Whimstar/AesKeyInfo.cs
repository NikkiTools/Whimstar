namespace Whimstar;

public record AesKeyInfo(string MainKey, List<AesKeyEntry> DynamicKeys);
