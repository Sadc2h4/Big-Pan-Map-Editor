using System.Globalization;

namespace PikminUnitEditor;

internal sealed record LayoutSpawn(
    int TypeId,
    string TypeLabel,
    float X,
    float Y,
    float Z,
    float Angle,
    float Radius,
    int MinCount,
    int MaxCount);

internal sealed record LayoutFile(IReadOnlyList<LayoutSpawn> Spawns);

internal sealed record RouteWaypoint(
    int Index,
    IReadOnlyList<int> Links,
    float X,
    float Y,
    float Z,
    float Radius);

internal sealed record RouteFile(IReadOnlyDictionary<int, RouteWaypoint> Waypoints);

internal sealed record WaterboxEntry(
    float X1,
    float Y1,
    float Z1,
    float X2,
    float Y2,
    float Z2)
{
    public float MinX => Math.Min(X1, X2);
    public float MaxX => Math.Max(X1, X2);
    public float MinY => Math.Min(Y1, Y2);
    public float MaxY => Math.Max(Y1, Y2);
    public float MinZ => Math.Min(Z1, Z2);
    public float MaxZ => Math.Max(Z1, Z2);
}

internal sealed record WaterboxFile(int Type, IReadOnlyList<WaterboxEntry> Boxes);

internal static class CaveAssetLocator
{
    //-------------------------------------------------------------------------------
    // unit 名から route/layout/waterbox の候補ファイルを探す処理
    //-------------------------------------------------------------------------------
    public static (string? LayoutPath, string? RoutePath, string? WaterboxPath) FindTextAssets(string arcRoot, string unitName)
    {
        string unitDir = Path.Combine(arcRoot, unitName);
        if (!Directory.Exists(unitDir))
        {
            return (null, null, null);
        }

        string? layoutPath = FindFirstExistingFile(
            Path.Combine(unitDir, "texts", "layout.txt"),
            Path.Combine(unitDir, "tmp", "layout.txt"),
            Path.Combine(unitDir, "大本", "texts", "layout.txt"));

        string? routePath = FindFirstExistingFile(
            Path.Combine(unitDir, "texts", "route.txt"),
            Path.Combine(unitDir, "tmp", "route.txt"),
            Path.Combine(unitDir, "大本", "texts", "route.txt"));

        string? waterboxPath = FindFirstExistingFile(
            Path.Combine(unitDir, "texts", "waterbox.txt"),
            Path.Combine(unitDir, "tmp", "waterbox.txt"),
            Path.Combine(unitDir, "大本", "texts", "waterbox.txt"));

        return (layoutPath, routePath, waterboxPath);
    }

    private static string? FindFirstExistingFile(params string[] candidates)
    {
        return candidates.FirstOrDefault(File.Exists);
    }
}

internal static class LayoutParser
{
    private static readonly IReadOnlyDictionary<int, string> TypeLabels = new Dictionary<int, string>
    {
        [0] = "Teki A",
        [1] = "Teki B",
        [2] = "Item",
        [3] = "Unused",
        [4] = "Hole/Geyser",  // ゲーム側 BaseGen::CaveGenType では 4 が HoleOrGeyser
        [5] = "Joint",        // 5 は Joint(DoorSeam) であり Hole/Geyser ではない
        [6] = "Plant",
        [7] = "Start",
        [8] = "Teki F"
    };

    //-------------------------------------------------------------------------------
    // layout.txt を最小構造で読み込む処理
    //-------------------------------------------------------------------------------
    public static LayoutFile ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new LayoutFile(Array.Empty<LayoutSpawn>());
        }

        List<string> lines = File.ReadAllLines(path)
            .Select(RemoveComment)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        List<LayoutSpawn> spawns = new();
        int index = 0;
        while (index < lines.Count)
        {
            if (lines[index] != "{")
            {
                index++;
                continue;
            }

            if (index + 6 >= lines.Count)
            {
                break;
            }

            int typeId = ParseInt(lines[index + 1]);
            float[] position = ParseFloats(lines[index + 2], 3);
            float angle = ParseFloat(lines[index + 3]);
            float radius = ParseFloat(lines[index + 4]);
            int minCount = ParseInt(lines[index + 5]);
            int maxCount = ParseInt(lines[index + 6]);

            spawns.Add(new LayoutSpawn(
                typeId,
                TypeLabels.TryGetValue(typeId, out string? label) ? label : $"Type {typeId}",
                position[0],
                position[1],
                position[2],
                angle,
                radius,
                minCount,
                maxCount));

            while (index < lines.Count && lines[index] != "}")
            {
                index++;
            }

            index++;
        }

        return new LayoutFile(spawns);
    }

    private static string RemoveComment(string line)
    {
        int commentIndex = line.IndexOf('#');
        return (commentIndex >= 0 ? line[..commentIndex] : line).Trim();
    }

    private static int ParseInt(string line)
    {
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
        return int.Parse(token, CultureInfo.InvariantCulture);
    }

    private static float ParseFloat(string line)
    {
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
        return float.Parse(token, CultureInfo.InvariantCulture);
    }

    private static float[] ParseFloats(string line, int count)
    {
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        float[] values = new float[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = float.Parse(tokens[i], CultureInfo.InvariantCulture);
        }

        return values;
    }
}

internal static class LayoutSerializer
{
    //-------------------------------------------------------------------------------
    // layout.txt を現在の spawn 構造から書き戻す処理
    //-------------------------------------------------------------------------------
    public static void WriteFile(string path, LayoutFile layout)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using StreamWriter writer = new(path, false);
        writer.WriteLine("# BaseGen file");
        writer.WriteLine($"{layout.Spawns.Count} \t # num gens");
        for (int index = 0; index < layout.Spawns.Count; index++)
        {
            LayoutSpawn spawn = layout.Spawns[index];
            writer.WriteLine($"# basegen {index}");
            writer.WriteLine("{");
            writer.WriteLine($"\t{spawn.TypeId} \t# type {spawn.TypeLabel}");
            writer.WriteLine(
                $"\t{spawn.X.ToString(CultureInfo.InvariantCulture)} " +
                $"{spawn.Y.ToString(CultureInfo.InvariantCulture)} " +
                $"{spawn.Z.ToString(CultureInfo.InvariantCulture)}\t# pos");
            writer.WriteLine($"\t{spawn.Angle.ToString(CultureInfo.InvariantCulture)} \t# ang");
            writer.WriteLine($"\t{spawn.Radius.ToString(CultureInfo.InvariantCulture)} \t# radius");
            writer.WriteLine($"\t{spawn.MinCount} \t# min num");
            writer.WriteLine($"\t{spawn.MaxCount} \t# max num");
            writer.WriteLine("}");
        }
    }
}

internal static class RouteParser
{
    //-------------------------------------------------------------------------------
    // route.txt を最小構造で読み込む処理
    //-------------------------------------------------------------------------------
    public static RouteFile ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new RouteFile(new Dictionary<int, RouteWaypoint>());
        }

        List<string> lines = File.ReadAllLines(path)
            .Select(RemoveComment)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        Dictionary<int, RouteWaypoint> waypoints = new();
        int index = 0;
        while (index < lines.Count)
        {
            if (lines[index] != "{")
            {
                index++;
                continue;
            }

            int waypointIndex = ParseInt(lines[index + 1]);
            int linkCount = ParseInt(lines[index + 2]);
            List<int> links = new();
            for (int i = 0; i < linkCount; i++)
            {
                links.Add(ParseInt(lines[index + 3 + i]));
            }

            float[] position = ParseFloats(lines[index + 3 + linkCount], 4);
            waypoints[waypointIndex] = new RouteWaypoint(
                waypointIndex,
                links,
                position[0],
                position[1],
                position[2],
                position[3]);

            while (index < lines.Count && lines[index] != "}")
            {
                index++;
            }

            index++;
        }

        return new RouteFile(waypoints);
    }

    private static string RemoveComment(string line)
    {
        int commentIndex = line.IndexOf('#');
        return (commentIndex >= 0 ? line[..commentIndex] : line).Trim();
    }

    private static int ParseInt(string line)
    {
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
        return int.Parse(token, CultureInfo.InvariantCulture);
    }

    private static float[] ParseFloats(string line, int count)
    {
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        float[] values = new float[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = float.Parse(tokens[i], CultureInfo.InvariantCulture);
        }

        return values;
    }
}

internal static class RouteSerializer
{
    //-------------------------------------------------------------------------------
    // route.txt を現在の waypoint 構造から書き戻す処理
    //-------------------------------------------------------------------------------
    public static void WriteFile(string path, RouteFile route)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using StreamWriter writer = new(path, false);
        IReadOnlyList<RouteWaypoint> orderedWaypoints = route.Waypoints.Values
            .OrderBy(waypoint => waypoint.Index)
            .ToList();

        writer.WriteLine($"{orderedWaypoints.Count} # waypoint count");
        foreach (RouteWaypoint waypoint in orderedWaypoints)
        {
            writer.WriteLine("{");
            writer.WriteLine($"\t{waypoint.Index} # index");
            writer.WriteLine($"\t{waypoint.Links.Count} # numLinks");
            for (int linkIndex = 0; linkIndex < waypoint.Links.Count; linkIndex++)
            {
                writer.WriteLine($"\t{waypoint.Links[linkIndex]} # link {linkIndex}");
            }

            writer.WriteLine(
                $"\t{waypoint.X.ToString(CultureInfo.InvariantCulture)} " +
                $"{waypoint.Y.ToString(CultureInfo.InvariantCulture)} " +
                $"{waypoint.Z.ToString(CultureInfo.InvariantCulture)} " +
                $"{waypoint.Radius.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine("}");
        }
    }
}

internal static class WaterboxParser
{
    //-------------------------------------------------------------------------------
    // waterbox.txt を AABB 構造として読み込む処理
    //-------------------------------------------------------------------------------
    public static WaterboxFile ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new WaterboxFile(0, Array.Empty<WaterboxEntry>());
        }

        List<string> lines = File.ReadAllLines(path)
            .Select(RemoveComment)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count == 0)
        {
            return new WaterboxFile(0, Array.Empty<WaterboxEntry>());
        }

        int type = ParseInt(lines[0]);
        int index = 1;
        while (index < lines.Count && lines[index] != "{")
        {
            index++;
        }

        if (index >= lines.Count - 1)
        {
            return new WaterboxFile(type, Array.Empty<WaterboxEntry>());
        }

        int count = ParseInt(lines[index + 1]);
        List<WaterboxEntry> boxes = new();
        index += 2;
        while (index < lines.Count && boxes.Count < count)
        {
            if (lines[index] == "}")
            {
                break;
            }

            float[] values = ParseFloats(lines[index], 6);
            boxes.Add(new WaterboxEntry(values[0], values[1], values[2], values[3], values[4], values[5]));
            index++;
        }

        return new WaterboxFile(type, boxes);
    }

    private static string RemoveComment(string line)
    {
        int commentIndex = line.IndexOf('#');
        return (commentIndex >= 0 ? line[..commentIndex] : line).Trim();
    }

    private static int ParseInt(string line)
    {
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
        return int.Parse(token, CultureInfo.InvariantCulture);
    }

    private static float[] ParseFloats(string line, int count)
    {
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        float[] values = new float[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = float.Parse(tokens[i], CultureInfo.InvariantCulture);
        }

        return values;
    }
}

internal static class WaterboxSerializer
{
    //-------------------------------------------------------------------------------
    // waterbox.txt を現在の AABB 構造から書き戻す処理
    //-------------------------------------------------------------------------------
    public static void WriteFile(string path, WaterboxFile waterbox)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using StreamWriter writer = new(path, false);
        writer.WriteLine($"{waterbox.Type} \t # type");
        writer.WriteLine("# CNode");
        writer.WriteLine("{");
        writer.WriteLine($"\t{waterbox.Boxes.Count} ");
        for (int index = 0; index < waterbox.Boxes.Count; index++)
        {
            WaterboxEntry box = waterbox.Boxes[index];
            writer.WriteLine(
                $"\t{box.X1.ToString(CultureInfo.InvariantCulture)} " +
                $"{box.Y1.ToString(CultureInfo.InvariantCulture)} " +
                $"{box.Z1.ToString(CultureInfo.InvariantCulture)} " +
                $"{box.X2.ToString(CultureInfo.InvariantCulture)} " +
                $"{box.Y2.ToString(CultureInfo.InvariantCulture)} " +
                $"{box.Z2.ToString(CultureInfo.InvariantCulture)} \t# {index}");
        }

        writer.WriteLine("}");
    }
}
