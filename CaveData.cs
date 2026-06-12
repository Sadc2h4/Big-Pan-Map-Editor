using System.Globalization;
using System.Text.RegularExpressions;

namespace PikminUnitEditor;

internal enum UnitKind
{
    Cap = 0,
    Room = 1,
    Hallway = 2
}

internal sealed record DoorLinkDefinition(float Distance, int DoorId, int TekiFlag);

internal sealed record DoorDefinition(
    int Index,
    int Direction,
    int Offset,
    int WayPointIndex,
    IReadOnlyList<DoorLinkDefinition> DoorLinks);

internal sealed record UnitDefinition(
    string Name,
    int Width,
    int Height,
    UnitKind Kind,
    int RoomFlagA,
    int RoomFlagB,
    IReadOnlyList<DoorDefinition> Doors);

internal sealed record UnitConnectionPoint(int DoorIndex, int Direction, int Offset, int WayPointIndex, float X, float Z);

internal static class UnitConnectionGeometry
{
    private const float CellSize = 170f;

    //-------------------------------------------------------------------------------
    // ユニット定義のドア情報からローカル接続座標一覧を作成する処理
    //-------------------------------------------------------------------------------
    public static IReadOnlyList<UnitConnectionPoint> GetConnectionPoints(UnitDefinition? unitDefinition)
    {
        if (unitDefinition is null)
        {
            return Array.Empty<UnitConnectionPoint>();
        }

        List<UnitConnectionPoint> points = new();
        foreach (DoorDefinition door in unitDefinition.Doors)
        {
            points.Add(CreateConnectionPoint(unitDefinition, door));
        }

        return points;
    }

    //-------------------------------------------------------------------------------
    // CaveGen の doorPos と同じ基準でドアのローカル接続座標を算出する処理
    //-------------------------------------------------------------------------------
    public static UnitConnectionPoint CreateConnectionPoint(UnitDefinition unitDefinition, DoorDefinition door)
    {
        float cellX = 0f;
        float cellZ = 0f;
        switch (door.Direction)
        {
            case 0:
                cellX = door.Offset + 0.5f;
                cellZ = 0f;
                break;
            case 1:
                cellX = unitDefinition.Width;
                cellZ = door.Offset + 0.5f;
                break;
            case 2:
                cellX = door.Offset + 0.5f;
                cellZ = unitDefinition.Height;
                break;
            case 3:
                cellX = 0f;
                cellZ = door.Offset + 0.5f;
                break;
        }

        float x = (cellX - (unitDefinition.Width * 0.5f)) * CellSize;
        float z = (cellZ - (unitDefinition.Height * 0.5f)) * CellSize;
        return new UnitConnectionPoint(door.Index, door.Direction, door.Offset, door.WayPointIndex, x, z);
    }

    //-------------------------------------------------------------------------------
    // ユニット外周に配置できる Door 候補座標を作成する処理
    //-------------------------------------------------------------------------------
    public static IReadOnlyList<UnitConnectionPoint> GetDoorPlacementCandidates(UnitDefinition? unitDefinition)
    {
        if (unitDefinition is null)
        {
            return Array.Empty<UnitConnectionPoint>();
        }

        List<UnitConnectionPoint> candidates = new();
        for (int offset = 0; offset < unitDefinition.Width; offset++)
        {
            candidates.Add(CreateConnectionPoint(unitDefinition, new DoorDefinition(-1, 0, offset, 0, Array.Empty<DoorLinkDefinition>())));
            candidates.Add(CreateConnectionPoint(unitDefinition, new DoorDefinition(-1, 2, offset, 0, Array.Empty<DoorLinkDefinition>())));
        }

        for (int offset = 0; offset < unitDefinition.Height; offset++)
        {
            candidates.Add(CreateConnectionPoint(unitDefinition, new DoorDefinition(-1, 1, offset, 0, Array.Empty<DoorLinkDefinition>())));
            candidates.Add(CreateConnectionPoint(unitDefinition, new DoorDefinition(-1, 3, offset, 0, Array.Empty<DoorLinkDefinition>())));
        }

        return candidates;
    }

    //-------------------------------------------------------------------------------
    // 任意座標から最も近い Door 配置候補を取得する処理
    //-------------------------------------------------------------------------------
    public static bool TryGetNearestDoorPlacement(UnitDefinition? unitDefinition, float x, float z, out UnitConnectionPoint placement)
    {
        UnitConnectionPoint? nearest = GetDoorPlacementCandidates(unitDefinition)
            .OrderBy(point => GetSquaredDistance(point.X, point.Z, x, z))
            .FirstOrDefault();
        if (nearest is null)
        {
            placement = default!;
            return false;
        }

        placement = nearest;
        return true;
    }

    //-------------------------------------------------------------------------------
    // X/Z 座標間の二乗距離を取得する処理
    //-------------------------------------------------------------------------------
    private static float GetSquaredDistance(float firstX, float firstZ, float secondX, float secondZ)
    {
        float dx = secondX - firstX;
        float dz = secondZ - firstZ;
        return (dx * dx) + (dz * dz);
    }
}

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

                int[] roomFlags = ExtractInts(lines[index++], 2);
                int doorCount = ExtractInts(lines[index++], 1)[0];

                List<DoorDefinition> doors = new();
                for (int i = 0; i < doorCount; i++)
                {
                    int doorIndex = ExtractInts(lines[index++], 1)[0];
                    int[] doorValues = ExtractInts(lines[index++], 3);
                    int linkCount = ExtractInts(lines[index++], 1)[0];

                    List<DoorLinkDefinition> doorLinks = new();
                    for (int j = 0; j < linkCount; j++)
                    {
                        string[] parts = ExtractParts(lines[index++]);
                        if (parts.Length >= 3 &&
                            float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float distance) &&
                            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int doorId) &&
                            int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tekiFlag))
                        {
                            doorLinks.Add(new DoorLinkDefinition(distance, doorId, tekiFlag));
                        }
                    }

                    doors.Add(new DoorDefinition(doorIndex, doorValues[0], doorValues[1], doorValues[2], doorLinks));
                }

                while (index < lines.Length && !lines[index].TrimStart().StartsWith("}", StringComparison.Ordinal))
                {
                    index++;
                }

                units.Add(new UnitDefinition(name, size[0], size[1], kind, roomFlags[0], roomFlags[1], doors));
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
        string[] parts = ExtractParts(line);

        int[] values = new int[count];
        for (int i = 0; i < count && i < parts.Length; i++)
        {
            values[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);
        }

        return values;
    }

    private static string[] ExtractParts(string line)
    {
        return line.Split('#')[0]
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }
}

internal static class UnitDefinitionSerializer
{
    //-------------------------------------------------------------------------------
    // UnitDefinition 一覧を all_units 互換形式で保存する処理
    //-------------------------------------------------------------------------------
    public static void WriteFile(string path, IReadOnlyList<UnitDefinition> units)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        using StreamWriter writer = new(path, false);
        writer.WriteLine("#");
        writer.WriteLine("#");
        writer.WriteLine("#\tunits definition file");
        writer.WriteLine("#");
        writer.WriteLine("#");
        writer.WriteLine($"{units.Count}\t# number of units");
        foreach (UnitDefinition unit in units)
        {
            WriteUnit(writer, unit);
        }
    }

    //-------------------------------------------------------------------------------
    // 1 件の UnitDefinition を all_units 互換形式で出力する処理
    //-------------------------------------------------------------------------------
    private static void WriteUnit(StreamWriter writer, UnitDefinition unit)
    {
        writer.WriteLine($"# {unit.Name}");
        writer.WriteLine("{");
        writer.WriteLine("\t1 \t# version");
        writer.WriteLine($"\t{unit.Name} \t# foldername");
        writer.WriteLine($"\t{unit.Width} {unit.Height} \t# dX/dZ ; cell size");
        writer.WriteLine($"\t{(int)unit.Kind} \t# room type");
        writer.WriteLine($"\t{unit.RoomFlagA} {unit.RoomFlagB} \t# room Flags");
        writer.WriteLine($"\t{unit.Doors.Count} \t# num doors");
        foreach (DoorDefinition door in unit.Doors.OrderBy(door => door.Index))
        {
            writer.WriteLine($"\t{door.Index} \t# index");
            writer.WriteLine($"\t{door.Direction} {door.Offset} {door.WayPointIndex} \t# dir/offs/wpindex");
            writer.WriteLine($"\t{door.DoorLinks.Count} \t# door links");
            foreach (DoorLinkDefinition link in door.DoorLinks)
            {
                writer.WriteLine(
                    $"\t{link.Distance.ToString("0.######", CultureInfo.InvariantCulture)} {link.DoorId} {link.TekiFlag} \t# dist/door-id/tekiflag");
            }
        }

        writer.WriteLine("}");
    }
}
