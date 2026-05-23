using System.Globalization;
using System.Text.RegularExpressions;

namespace PikminUnitEditor;

internal enum UnitKind
{
    Cap = 0,
    Room = 1,
    Hallway = 2
}

internal sealed record DoorDefinition(int Index, int Direction, int Offset, int WayPointIndex);

internal sealed record UnitDefinition(
    string Name,
    int Width,
    int Height,
    UnitKind Kind,
    IReadOnlyList<DoorDefinition> Doors);

internal sealed record FloorInfo(
    int FloorIndex,
    int RoomCount,
    float RouteRatio,
    int CapMaxPercent,
    string UnitSetFile,
    IReadOnlyDictionary<string, string> Properties);

internal sealed record CaveInfoFile(IReadOnlyList<FloorInfo> Floors);

internal static class CaveInfoParser
{
    public static CaveInfoFile ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new CaveInfoFile(Array.Empty<FloorInfo>());
        }

        List<FloorInfo> floors = new();
        string[] lines = File.ReadAllLines(path);
        int currentFloorIndex = 0;

        while (currentFloorIndex < lines.Length)
        {
            if (!lines[currentFloorIndex].Contains("# FloorInfo", StringComparison.OrdinalIgnoreCase))
            {
                currentFloorIndex++;
                continue;
            }

            Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
            currentFloorIndex += 2;

            while (currentFloorIndex < lines.Length && !lines[currentFloorIndex].Contains("{_eof}", StringComparison.OrdinalIgnoreCase))
            {
                Match match = Regex.Match(lines[currentFloorIndex], @"\{(?<key>f[0-9A-Fa-f]+)\}\s+[^\s]+\s+(?<value>.+?)\s+#");
                if (match.Success)
                {
                    values[match.Groups["key"].Value] = match.Groups["value"].Value.Trim();
                }

                currentFloorIndex++;
            }

            floors.Add(new FloorInfo(
                ParseInt(values, "f000"),
                Math.Max(ParseInt(values, "f005"), 1),
                ParseFloat(values, "f006"),
                ParseInt(values, "f014"),
                values.TryGetValue("f008", out string? unitSet) ? unitSet : "all_units.txt",
                new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase)));
        }

        return new CaveInfoFile(floors);
    }

    private static int ParseInt(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed) ? parsed : 0;
    }

    private static float ParseFloat(IReadOnlyDictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out string? value) &&
               float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : 0f;
    }
}

internal static class UnitDefinitionParser
{
    public static IReadOnlyList<UnitDefinition> ParseMany(string path)
    {
        List<UnitDefinition> units = new();
        string[] lines = File.ReadAllLines(path);
        int index = 0;

        while (index < lines.Length)
        {
            if (!lines[index].TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                index++;
                continue;
            }

            try
            {
                index++; // {
                index++; // version

                string name = ExtractValue(lines[index++]);
                int[] size = ExtractInts(lines[index++], 2);
                UnitKind kind = (UnitKind)ExtractInts(lines[index++], 1)[0];

                index++; // room Flags
                int doorCount = ExtractInts(lines[index++], 1)[0];

                List<DoorDefinition> doors = new();
                for (int i = 0; i < doorCount; i++)
                {
                    int doorIndex = ExtractInts(lines[index++], 1)[0];
                    int[] doorValues = ExtractInts(lines[index++], 3);
                    int linkCount = ExtractInts(lines[index++], 1)[0];

                    for (int j = 0; j < linkCount; j++)
                    {
                        index++;
                    }

                    doors.Add(new DoorDefinition(doorIndex, doorValues[0], doorValues[1], doorValues[2]));
                }

                while (index < lines.Length && !lines[index].TrimStart().StartsWith("}", StringComparison.Ordinal))
                {
                    index++;
                }

                units.Add(new UnitDefinition(name, size[0], size[1], kind, doors));
            }
            catch (Exception ex)
            {
                throw new FormatException($"UnitDefinition の解析に失敗しました．line={index + 1}, text='{lines[Math.Min(index, lines.Length - 1)].Trim()}'", ex);
            }
            index++;
        }

        return units;
    }

    private static string ExtractValue(string line)
    {
        return line.Split('#')[0].Trim();
    }

    private static int[] ExtractInts(string line, int count)
    {
        string[] parts = line.Split('#')[0]
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        int[] values = new int[count];
        for (int i = 0; i < count && i < parts.Length; i++)
        {
            values[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);
        }

        return values;
    }
}
