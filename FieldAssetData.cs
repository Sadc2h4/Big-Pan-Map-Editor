using System.Globalization;

namespace PikminUnitEditor;

internal sealed record FieldMapData(
    string Name,
    string MapDirectory,
    string? TextsArchiveDirectory,
    RouteFile Route,
    IReadOnlyList<FieldGeneratorFile> GeneratorFiles);

internal sealed record FieldGeneratorFile(
    string Path,
    string DisplayName,
    FieldGeneratorScope Scope,
    int? StartDay,
    int? EndDay,
    int DeclaredObjectCount,
    IReadOnlyList<string> Lines,
    IReadOnlyList<FieldGeneratorObject> Objects);

internal enum FieldGeneratorScope
{
    Root,
    Loop,
    NonLoop
}

internal sealed record FieldGeneratorObject(
    string SourceFile,
    int SourceIndex,
    string ObjectType,
    string ObjectLabel,
    float X,
    float Y,
    float Z,
    float Angle,
    float Radius,
    int StartLineIndex,
    int EndLineIndex,
    int PositionLineIndex,
    int? AngleLineIndex,
    int? RadiusLineIndex);

internal sealed record FieldGeneratorBlock(
    IReadOnlyList<string> Lines,
    IReadOnlyList<int> OriginalLineIndices);

internal static class FieldAssetLocator
{
    //-------------------------------------------------------------------------------
    // 指定ルートから地上マップフォルダを解決する処理
    //-------------------------------------------------------------------------------
    public static string? ResolveFieldMapRoot(string selectedRoot)
    {
        string direct = Path.Combine(selectedRoot, "user", "Abe", "map");
        if (Directory.Exists(direct))
        {
            return direct;
        }

        string filesRoot = Path.Combine(selectedRoot, "files", "user", "Abe", "map");
        if (Directory.Exists(filesRoot))
        {
            return filesRoot;
        }

        string rootChild = Path.Combine(selectedRoot, "root", "user", "Abe", "map");
        if (Directory.Exists(rootChild))
        {
            return rootChild;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 指定ルートから地上 texts アーカイブフォルダを解決する処理
    //-------------------------------------------------------------------------------
    public static string? ResolveFieldTextsRoot(string selectedRoot)
    {
        string direct = Path.Combine(selectedRoot, "user", "Kando", "map");
        if (Directory.Exists(direct))
        {
            return direct;
        }

        string filesRoot = Path.Combine(selectedRoot, "files", "user", "Kando", "map");
        if (Directory.Exists(filesRoot))
        {
            return filesRoot;
        }

        string rootChild = Path.Combine(selectedRoot, "root", "user", "Kando", "map");
        if (Directory.Exists(rootChild))
        {
            return rootChild;
        }

        return null;
    }
}

internal static class FieldMapLoader
{
    private static readonly string[] RootGeneratorFiles =
    {
        "initgen.txt",
        "defaultgen.txt",
        "plantsgen.txt"
    };

    //-------------------------------------------------------------------------------
    // 地上マップフォルダから route と generator 概要を読み込む処理
    //-------------------------------------------------------------------------------
    public static FieldMapData Load(string fieldMapRoot, string? fieldTextsRoot, string mapName)
    {
        string mapDirectory = Path.Combine(fieldMapRoot, mapName);
        string routePath = Path.Combine(mapDirectory, "route.txt");
        RouteFile route = RouteParser.ParseFile(routePath);
        List<FieldGeneratorFile> generatorFiles = new();

        foreach (string fileName in RootGeneratorFiles)
        {
            string path = Path.Combine(mapDirectory, fileName);
            if (File.Exists(path))
            {
                generatorFiles.Add(FieldGeneratorParser.ParseFile(path, fileName));
            }
        }

        foreach (string folderName in new[] { "loop", "nonloop" })
        {
            string folderPath = Path.Combine(mapDirectory, folderName);
            if (!Directory.Exists(folderPath))
            {
                continue;
            }

            foreach (string path in Directory.GetFiles(folderPath, "*.txt").OrderBy(Path.GetFileName))
            {
                generatorFiles.Add(FieldGeneratorParser.ParseFile(path, Path.Combine(folderName, Path.GetFileName(path))));
            }
        }

        string? textsArchiveDirectory = fieldTextsRoot is null ? null : Path.Combine(fieldTextsRoot, mapName);
        if (!string.IsNullOrWhiteSpace(textsArchiveDirectory) && !Directory.Exists(textsArchiveDirectory))
        {
            textsArchiveDirectory = null;
        }

        return new FieldMapData(mapName, mapDirectory, textsArchiveDirectory, route, generatorFiles);
    }
}

internal static class FieldGeneratorParser
{
    //-------------------------------------------------------------------------------
    // 地上 generator ファイルを表示用の最小構造として読み込む処理
    //-------------------------------------------------------------------------------
    public static FieldGeneratorFile ParseFile(string path, string displayName)
    {
        string[] lines = File.ReadAllLines(path);
        return ParseLines(path, displayName, lines);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator 行一覧を表示用の最小構造として読み込む処理
    //-------------------------------------------------------------------------------
    private static FieldGeneratorFile ParseLines(string path, string displayName, IReadOnlyList<string> lines)
    {
        List<string> cleanedLines = GetCleanedLines(lines)
            .Select(entry => entry.Line)
            .ToList();

        int declaredCount = TryReadDeclaredCount(cleanedLines);
        FieldGeneratorScope scope = ParseScope(displayName);
        (int? startDay, int? endDay) = ParseDayRange(displayName);
        List<FieldGeneratorBlock> objectBlocks = ExtractTopLevelBlocks(lines);
        List<FieldGeneratorObject> objects = new();

        for (int i = 0; i < objectBlocks.Count; i++)
        {
            FieldGeneratorObject? parsed = TryParseObject(objectBlocks[i], displayName, i);
            if (parsed is not null)
            {
                objects.Add(parsed);
            }
        }

        return new FieldGeneratorFile(path, displayName, scope, startDay, endDay, declaredCount, lines.ToArray(), objects);
    }

    //-------------------------------------------------------------------------------
    // generator 表示名から日数条件の種類を取得する処理
    //-------------------------------------------------------------------------------
    private static FieldGeneratorScope ParseScope(string displayName)
    {
        string normalized = displayName.Replace('\\', '/');
        if (normalized.StartsWith("loop/", StringComparison.OrdinalIgnoreCase))
        {
            return FieldGeneratorScope.Loop;
        }

        if (normalized.StartsWith("nonloop/", StringComparison.OrdinalIgnoreCase))
        {
            return FieldGeneratorScope.NonLoop;
        }

        return FieldGeneratorScope.Root;
    }

    //-------------------------------------------------------------------------------
    // generator 表示名から日数範囲を取得する処理
    //-------------------------------------------------------------------------------
    private static (int? StartDay, int? EndDay) ParseDayRange(string displayName)
    {
        string fileName = Path.GetFileNameWithoutExtension(displayName);
        string[] parts = fileName.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int startDay) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int endDay))
        {
            return (null, null);
        }

        return (Math.Min(startDay, endDay), Math.Max(startDay, endDay));
    }

    //-------------------------------------------------------------------------------
    // generator の宣言数を取得する処理
    //-------------------------------------------------------------------------------
    private static int TryReadDeclaredCount(IReadOnlyList<string> lines)
    {
        int declaredCount = 0;
        foreach (string line in lines)
        {
            if (line == "{")
            {
                break;
            }

            if (TryParseLeadingInt(line, out int value))
            {
                declaredCount = value;
            }
        }

        return declaredCount;
    }

    //-------------------------------------------------------------------------------
    // generator ファイルからトップレベルの object ブロックを抽出する処理
    //-------------------------------------------------------------------------------
    private static List<FieldGeneratorBlock> ExtractTopLevelBlocks(IReadOnlyList<string> lines)
    {
        List<FieldGeneratorBlock> blocks = new();
        List<(string Line, int OriginalLineIndex)> cleanedLines = GetCleanedLines(lines);
        List<string>? current = null;
        List<int>? currentIndices = null;
        int depth = 0;

        foreach ((string line, int originalLineIndex) in cleanedLines)
        {
            if (line == "{")
            {
                if (depth == 0)
                {
                    current = new List<string>();
                    currentIndices = new List<int>();
                }

                depth++;
            }

            current?.Add(line);
            currentIndices?.Add(originalLineIndex);

            if (line == "}" && depth > 0)
            {
                depth--;
                if (depth == 0 && current is not null && currentIndices is not null)
                {
                    blocks.Add(new FieldGeneratorBlock(current, currentIndices));
                    current = null;
                    currentIndices = null;
                }
            }
        }

        return blocks;
    }

    //-------------------------------------------------------------------------------
    // object ブロックから表示用の位置と種別を取得する処理
    //-------------------------------------------------------------------------------
    private static FieldGeneratorObject? TryParseObject(FieldGeneratorBlock block, string sourceFile, int sourceIndex)
    {
        if (block.Lines.Count < 8)
        {
            return null;
        }

        float[] position = TryParseFloatLine(block.Lines[5], 3);
        if (position.Length < 3)
        {
            return null;
        }

        string typeLine = block.Lines[7];
        string objectType = ParseObjectType(typeLine);
        string label = BuildObjectLabel(objectType, block.Lines);
        int? angleLineIndex = TryFindAngleLineIndex(objectType, block.Lines);
        int? radiusLineIndex = TryFindRadiusLineIndex(objectType, block.Lines);
        float angle = angleLineIndex is null ? 0f : TryFindAngle(objectType, block.Lines);
        float radius = radiusLineIndex is null ? 0f : TryFindRadius(objectType, block.Lines);

        return new FieldGeneratorObject(
            sourceFile,
            sourceIndex,
            objectType,
            label,
            position[0],
            position[1],
            position[2],
            angle,
            radius,
            block.OriginalLineIndices.First(),
            block.OriginalLineIndices.Last(),
            block.OriginalLineIndices[5],
            angleLineIndex is null ? null : block.OriginalLineIndices[angleLineIndex.Value],
            radiusLineIndex is null ? null : block.OriginalLineIndices[radiusLineIndex.Value]);
    }

    //-------------------------------------------------------------------------------
    // 元ファイルのコメントを保ったまま generator object の編集内容を書き戻す処理
    //-------------------------------------------------------------------------------
    public static void WriteFile(FieldGeneratorFile generatorFile)
    {
        string[] lines = generatorFile.Lines.ToArray();
        foreach (FieldGeneratorObject fieldObject in generatorFile.Objects)
        {
            if (fieldObject.PositionLineIndex >= 0 && fieldObject.PositionLineIndex < lines.Length)
            {
                lines[fieldObject.PositionLineIndex] = ReplaceLeadingNumbers(
                    lines[fieldObject.PositionLineIndex],
                    fieldObject.X,
                    fieldObject.Y,
                    fieldObject.Z);
            }

            if (fieldObject.AngleLineIndex is int angleLineIndex &&
                angleLineIndex >= 0 &&
                angleLineIndex < lines.Length)
            {
                lines[angleLineIndex] = fieldObject.ObjectType switch
                {
                    "{item}" or "{pelt}" => ReplaceLeadingNumbers(lines[angleLineIndex], null, fieldObject.Angle, null),
                    "{teki}" => ReplaceLeadingNumbers(lines[angleLineIndex], fieldObject.Angle),
                    _ => lines[angleLineIndex]
                };
            }

            if (fieldObject.RadiusLineIndex is int radiusLineIndex &&
                radiusLineIndex >= 0 &&
                radiusLineIndex < lines.Length)
            {
                lines[radiusLineIndex] = ReplaceLeadingNumbers(lines[radiusLineIndex], Math.Max(0f, fieldObject.Radius));
            }
        }

        File.WriteAllLines(generatorFile.Path, lines);
    }

    //-------------------------------------------------------------------------------
    // 指定 object の元 generator テキストを取得する処理
    //-------------------------------------------------------------------------------
    public static string GetObjectRawText(FieldGeneratorFile generatorFile, int objectIndex)
    {
        if (objectIndex < 0 || objectIndex >= generatorFile.Objects.Count)
        {
            return string.Empty;
        }

        FieldGeneratorObject fieldObject = generatorFile.Objects[objectIndex];
        return string.Join(
            Environment.NewLine,
            generatorFile.Lines
                .Skip(fieldObject.StartLineIndex)
                .Take(fieldObject.EndLineIndex - fieldObject.StartLineIndex + 1));
    }

    //-------------------------------------------------------------------------------
    // 指定 object の generator テキストを差し替えて再解析する処理
    //-------------------------------------------------------------------------------
    public static FieldGeneratorFile ReplaceObjectRawText(FieldGeneratorFile generatorFile, int objectIndex, string rawText)
    {
        if (objectIndex < 0 || objectIndex >= generatorFile.Objects.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(objectIndex));
        }

        List<string> replacementLines = NormalizeRawTextLines(rawText);
        FieldGeneratorBlock replacementBlock = CreateRawTextBlock(replacementLines);
        FieldGeneratorObject? parsed = TryParseObject(replacementBlock, generatorFile.DisplayName, objectIndex);
        if (parsed is null)
        {
            throw new InvalidDataException("generator object として解析できませんでした．");
        }

        FieldGeneratorObject oldObject = generatorFile.Objects[objectIndex];
        List<string> lines = generatorFile.Lines.ToList();
        int removeCount = oldObject.EndLineIndex - oldObject.StartLineIndex + 1;
        lines.RemoveRange(oldObject.StartLineIndex, removeCount);
        lines.InsertRange(oldObject.StartLineIndex, replacementLines);
        UpdateDeclaredObjectCount(lines, generatorFile.Objects.Count);
        return ParseLines(generatorFile.Path, generatorFile.DisplayName, lines);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator ファイルへ raw object テキストを追加して再解析する処理
    //-------------------------------------------------------------------------------
    public static FieldGeneratorFile AddObjectRawText(FieldGeneratorFile generatorFile, string rawText)
    {
        List<string> replacementLines = NormalizeRawTextLines(rawText);
        FieldGeneratorBlock replacementBlock = CreateRawTextBlock(replacementLines);
        FieldGeneratorObject? parsed = TryParseObject(replacementBlock, generatorFile.DisplayName, generatorFile.Objects.Count);
        if (parsed is null)
        {
            throw new InvalidDataException("generator object として解析できませんでした．");
        }

        List<string> lines = generatorFile.Lines.ToList();
        if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.Add(string.Empty);
        }

        lines.AddRange(replacementLines);
        UpdateDeclaredObjectCount(lines, generatorFile.Objects.Count + 1);
        return ParseLines(generatorFile.Path, generatorFile.DisplayName, lines);
    }

    //-------------------------------------------------------------------------------
    // 地上 generator ファイルから指定 object を削除して再解析する処理
    //-------------------------------------------------------------------------------
    public static FieldGeneratorFile RemoveObject(FieldGeneratorFile generatorFile, int objectIndex)
    {
        if (objectIndex < 0 || objectIndex >= generatorFile.Objects.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(objectIndex));
        }

        FieldGeneratorObject oldObject = generatorFile.Objects[objectIndex];
        List<string> lines = generatorFile.Lines.ToList();
        int removeCount = oldObject.EndLineIndex - oldObject.StartLineIndex + 1;
        lines.RemoveRange(oldObject.StartLineIndex, removeCount);
        UpdateDeclaredObjectCount(lines, Math.Max(0, generatorFile.Objects.Count - 1));
        return ParseLines(generatorFile.Path, generatorFile.DisplayName, lines);
    }

    //-------------------------------------------------------------------------------
    // raw editor のテキストを行一覧へ正規化する処理
    //-------------------------------------------------------------------------------
    private static List<string> NormalizeRawTextLines(string rawText)
    {
        return rawText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .ToList();
    }

    //-------------------------------------------------------------------------------
    // raw editor の行一覧から仮 object ブロックを作成する処理
    //-------------------------------------------------------------------------------
    private static FieldGeneratorBlock CreateRawTextBlock(IReadOnlyList<string> lines)
    {
        List<string> cleanedLines = new();
        List<int> indices = new();
        for (int i = 0; i < lines.Count; i++)
        {
            string line = RemoveComment(lines[i]);
            if (!string.IsNullOrWhiteSpace(line))
            {
                cleanedLines.Add(line);
                indices.Add(i);
            }
        }

        return new FieldGeneratorBlock(cleanedLines, indices);
    }

    //-------------------------------------------------------------------------------
    // generator ヘッダーの object 数を現在数へ更新する処理
    //-------------------------------------------------------------------------------
    private static void UpdateDeclaredObjectCount(List<string> lines, int objectCount)
    {
        int? countLineIndex = null;
        for (int i = 0; i < lines.Count; i++)
        {
            string cleaned = RemoveComment(lines[i]);
            if (cleaned == "{")
            {
                break;
            }

            if (TryParseLeadingInt(cleaned, out _))
            {
                countLineIndex = i;
            }
        }

        if (countLineIndex is int index)
        {
            lines[index] = ReplaceLeadingNumbers(lines[index], objectCount);
        }
    }

    //-------------------------------------------------------------------------------
    // object 種別行から先頭トークンを取得する処理
    //-------------------------------------------------------------------------------
    private static string ParseObjectType(string line)
    {
        return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "{unknown}";
    }

    //-------------------------------------------------------------------------------
    // object 種別から表示名を組み立てる処理
    //-------------------------------------------------------------------------------
    private static string BuildObjectLabel(string objectType, IReadOnlyList<string> block)
    {
        if (objectType == "{item}")
        {
            int subtypeIndex = FindItemSubtypeIndex(block, objectType);
            string subtype = subtypeIndex >= 0 ? block[subtypeIndex] : "{item}";
            return subtype switch
            {
                "{brdg}" => GetBridgeLabel(block, subtypeIndex),
                "{gate}" => "Gate",
                "{dgat}" => "Electric Gate",
                "{dwfl}" => GetDownFloorLabel(block, subtypeIndex),
                "{onyn}" => GetOnionLabel(block, subtypeIndex),
                "{cave}" => "Cave Entrance",
                "{plnt}" => "Plant",
                _ => "Item"
            };
        }

        return objectType switch
        {
            "{teki}" => "Teki",
            "{pelt}" => "Pellet / Treasure",
            "{piki}" => "Pikmin",
            _ => objectType
        };
    }

    //-------------------------------------------------------------------------------
    // item ブロック内の subtype 行を探す処理
    //-------------------------------------------------------------------------------
    private static int FindItemSubtypeIndex(IReadOnlyList<string> block, string objectType)
    {
        return FindLineIndex(block, line =>
            line.StartsWith("{", StringComparison.Ordinal) &&
            line != "{" &&
            line != "}" &&
            line != "{v0.1}" &&
            line != "{v0.3}" &&
            !line.StartsWith(objectType + " ", StringComparison.Ordinal) &&
            line != objectType);
    }

    //-------------------------------------------------------------------------------
    // 橋タイプから表示名を取得する処理
    //-------------------------------------------------------------------------------
    private static string GetBridgeLabel(IReadOnlyList<string> block, int subtypeIndex)
    {
        if (subtypeIndex >= 0 && subtypeIndex + 3 < block.Count && TryParseLeadingInt(block[subtypeIndex + 3], out int bridgeType))
        {
            return bridgeType switch
            {
                0 => "Short Bridge",
                1 => "Short Bridge (Slanted)",
                2 => "Long Bridge",
                _ => "Bridge"
            };
        }

        return "Bridge";
    }

    //-------------------------------------------------------------------------------
    // 袋や沈む床タイプから表示名を取得する処理
    //-------------------------------------------------------------------------------
    private static string GetDownFloorLabel(IReadOnlyList<string> block, int subtypeIndex)
    {
        if (subtypeIndex >= 0 &&
            subtypeIndex + 5 < block.Count &&
            TryParseLeadingInt(block[subtypeIndex + 4], out int floorType) &&
            TryParseLeadingInt(block[subtypeIndex + 5], out int behavior))
        {
            if (floorType == 2)
            {
                return "Paper Bag";
            }

            string sizeName = floorType == 0 ? "Small Block" : "Normal Block";
            return behavior == 1 ? $"{sizeName} [Seesaw]" : sizeName;
        }

        return "Down Floor / Paper Bag";
    }

    //-------------------------------------------------------------------------------
    // Onion / Rocket タイプから表示名を取得する処理
    //-------------------------------------------------------------------------------
    private static string GetOnionLabel(IReadOnlyList<string> block, int subtypeIndex)
    {
        if (subtypeIndex >= 0 && subtypeIndex + 3 < block.Count && TryParseLeadingInt(block[subtypeIndex + 3], out int onionType))
        {
            return onionType switch
            {
                0 => "Blue Onion",
                1 => "Red Onion",
                2 => "Yellow Onion",
                4 or 8 => "Rocket",
                _ => "Onion / Rocket"
            };
        }

        return "Onion / Rocket";
    }

    //-------------------------------------------------------------------------------
    // object 種別から表示用 Spawn Type を割り当てる処理
    //-------------------------------------------------------------------------------
    public static int ToDisplayTypeId(FieldGeneratorObject fieldObject)
    {
        if (fieldObject.ObjectLabel.Contains("Plant", StringComparison.OrdinalIgnoreCase))
        {
            return 6;
        }

        return fieldObject.ObjectType switch
        {
            "{teki}" => 0,
            "{piki}" => 7,
            "{pelt}" => 2,
            "{item}" => 2,
            _ => 2
        };
    }

    //-------------------------------------------------------------------------------
    // object ブロックから角度らしい値を取得する処理
    //-------------------------------------------------------------------------------
    private static float TryFindAngle(string objectType, IReadOnlyList<string> block)
    {
        if (objectType is "{item}" or "{pelt}")
        {
            int? rotationLineIndex = TryFindAngleLineIndex(objectType, block);
            if (rotationLineIndex is not null)
            {
                float[] rotation = TryParseFloatLine(block[rotationLineIndex.Value], 3);
                if (rotation.Length >= 2)
                {
                    return rotation[1];
                }
            }

            return 0f;
        }

        if (objectType != "{teki}")
        {
            return 0f;
        }

        int? angleLineIndex = TryFindAngleLineIndex(objectType, block);
        if (angleLineIndex is not null && TryParseLeadingFloat(block[angleLineIndex.Value], out float angle))
        {
            return angle;
        }

        return 0f;
    }

    //-------------------------------------------------------------------------------
    // object ブロックから半径らしい値を取得する処理
    //-------------------------------------------------------------------------------
    private static float TryFindRadius(string objectType, IReadOnlyList<string> block)
    {
        int? radiusLineIndex = TryFindRadiusLineIndex(objectType, block);
        if (radiusLineIndex is null)
        {
            return 0f;
        }

        if (TryParseLeadingFloat(block[radiusLineIndex.Value], out float radius))
        {
            return Math.Max(0f, radius);
        }

        return 0f;
    }

    //-------------------------------------------------------------------------------
    // object ブロック内で角度を持つ行番号を取得する処理
    //-------------------------------------------------------------------------------
    private static int? TryFindAngleLineIndex(string objectType, IReadOnlyList<string> block)
    {
        if (objectType is "{item}" or "{pelt}")
        {
            int subtypeIndex = FindItemSubtypeIndex(block, objectType);
            return subtypeIndex >= 0 && subtypeIndex + 1 < block.Count ? subtypeIndex + 1 : null;
        }

        if (objectType == "{teki}")
        {
            int typeIndex = FindLineIndex(block, line => line.StartsWith("{teki}", StringComparison.Ordinal));
            return typeIndex >= 0 && typeIndex + 3 < block.Count ? typeIndex + 3 : null;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // object ブロック内で Radius を持つ行番号を取得する処理
    //-------------------------------------------------------------------------------
    private static int? TryFindRadiusLineIndex(string objectType, IReadOnlyList<string> block)
    {
        if (objectType != "{teki}")
        {
            return null;
        }

        int typeIndex = FindLineIndex(block, line => line.StartsWith("{teki}", StringComparison.Ordinal));
        return typeIndex >= 0 && typeIndex + 5 < block.Count ? typeIndex + 5 : null;
    }

    //-------------------------------------------------------------------------------
    // 条件に一致する行番号を取得する処理
    //-------------------------------------------------------------------------------
    private static int FindLineIndex(IReadOnlyList<string> block, Func<string, bool> predicate)
    {
        for (int i = 0; i < block.Count; i++)
        {
            if (predicate(block[i]))
            {
                return i;
            }
        }

        return -1;
    }

    //-------------------------------------------------------------------------------
    // 行コメントを除去する処理
    //-------------------------------------------------------------------------------
    private static string RemoveComment(string line)
    {
        int commentIndex = line.IndexOf('#');
        return (commentIndex >= 0 ? line[..commentIndex] : line).Trim();
    }

    //-------------------------------------------------------------------------------
    // 元行番号を保持したままコメント除去済み行一覧を作成する処理
    //-------------------------------------------------------------------------------
    private static List<(string Line, int OriginalLineIndex)> GetCleanedLines(IReadOnlyList<string> lines)
    {
        List<(string Line, int OriginalLineIndex)> cleanedLines = new();
        for (int i = 0; i < lines.Count; i++)
        {
            string line = RemoveComment(lines[i]);
            if (!string.IsNullOrWhiteSpace(line))
            {
                cleanedLines.Add((line, i));
            }
        }

        return cleanedLines;
    }

    //-------------------------------------------------------------------------------
    // 行頭の整数を取得する処理
    //-------------------------------------------------------------------------------
    private static bool TryParseLeadingInt(string line, out int value)
    {
        value = 0;
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    //-------------------------------------------------------------------------------
    // 行頭の実数を取得する処理
    //-------------------------------------------------------------------------------
    private static bool TryParseLeadingFloat(string line, out float value)
    {
        value = 0f;
        string token = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    //-------------------------------------------------------------------------------
    // 行から指定数の実数を取得する処理
    //-------------------------------------------------------------------------------
    private static float[] TryParseFloatLine(string line, int count)
    {
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < count)
        {
            return Array.Empty<float>();
        }

        float[] values = new float[count];
        for (int i = 0; i < count; i++)
        {
            if (!float.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
            {
                return Array.Empty<float>();
            }
        }

        return values;
    }

    //-------------------------------------------------------------------------------
    // 行頭の数値だけを差し替えてコメントとインデントを保持する処理
    //-------------------------------------------------------------------------------
    private static string ReplaceLeadingNumbers(string line, params float?[] values)
    {
        int commentIndex = line.IndexOf('#');
        string valuePart = commentIndex >= 0 ? line[..commentIndex] : line;
        string commentPart = commentIndex >= 0 ? line[commentIndex..] : string.Empty;
        string indent = valuePart[..valuePart.TakeWhile(char.IsWhiteSpace).Count()];
        string[] tokens = valuePart.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < values.Length && i < tokens.Length; i++)
        {
            if (values[i] is float value)
            {
                tokens[i] = value.ToString("0.######", CultureInfo.InvariantCulture);
            }
        }

        string trailing = string.IsNullOrWhiteSpace(commentPart) ? string.Empty : " " + commentPart.TrimEnd();
        return indent + string.Join(" ", tokens) + trailing;
    }
}
