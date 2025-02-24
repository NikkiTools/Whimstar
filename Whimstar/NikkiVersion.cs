namespace Whimstar;

public readonly record struct NikkiVersion(int Major, int Hotfix) : IComparable<NikkiVersion>
{
    public static NikkiVersion Parse(string version)
    {
        if (!TryParse(version, out var parsed))
            throw new InvalidOperationException($"Failed to parse version string {version}");

        return parsed;
    }

    public static bool TryParse(string version, out NikkiVersion parsedVersion)
    {
        parsedVersion = default;

        var parts = version.Split(".");

        switch (parts.Length)
        {
            case 1:
                if (!int.TryParse(parts[0], out var versionNumber))
                    return false;

                parsedVersion = new NikkiVersion(versionNumber, 0);
                break;
            case 2:
                var major = parts[0];
                var hotfix = parts[1];

                if (!int.TryParse(major, out var majorVersion) ||
                    !int.TryParse(hotfix, out var hotfixVersion))
                    return false;

                parsedVersion = new NikkiVersion(majorVersion, hotfixVersion);
                break;
            default:
                return false;
        }

        return true;
    }

    public override string ToString() => $"{Major}.{Hotfix}";

    public int CompareTo(NikkiVersion other)
    {
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
            return majorComparison;

        return Hotfix.CompareTo(other.Hotfix);
    }

    public static bool operator <(NikkiVersion left, NikkiVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(NikkiVersion left, NikkiVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(NikkiVersion left, NikkiVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(NikkiVersion left, NikkiVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
}