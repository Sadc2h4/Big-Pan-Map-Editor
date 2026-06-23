using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PikminUnitEditor;

//-------------------------------------------------------------------------------
// テストマップ生成の結果を表すレコード
//-------------------------------------------------------------------------------
internal sealed record TestCaveBuildResult(bool Success, string Message, IReadOnlyList<string> WrittenFiles);

//-------------------------------------------------------------------------------
// テスト洞窟作成時に反映する可変オプションを表すレコード
//-------------------------------------------------------------------------------
internal sealed record TestCaveBuildOptions(
    int BluePikminCount,
    int RedPikminCount,
    int YellowPikminCount,
    int PurplePikminCount,
    int WhitePikminCount,
    int BulbminCount,
    int CarrotCount,
    int BitterSprayCount,
    int SpicySprayCount)
{
    public static TestCaveBuildOptions Default { get; } = new(10, 10, 10, 10, 10, 0, 0, 2, 2);
}

//-------------------------------------------------------------------------------
// 編集中マップユニットを含む 1 階層のテスト用洞窟を生成し，
// チャレンジモード 0 面(ch_ABEM_tutorial)へ一括登録する処理群
//
// ゲーム側(pikmin2-main)は 1 つのチャレンジステージを下記 3 種のデータで参照し，
// いずれか 1 つでも階層数などが不整合だと読込時にフリーズするため，
// caveinfo / stages.txt / ChallengeBgmList.txt を必ず同時に整合させて書き出す．
//   ・caveinfo (ch_ABEM_tutorial.txt) : 洞窟の階層数とユニット指定
//   ・stages.txt                       : 開始ピクミン数 / スプレー数 / 階層数 / 階層時間
//   ・ChallengeBgmList.txt             : 階層ごとのBGM(階層数は caveinfo 以上が必須)
//-------------------------------------------------------------------------------
internal static class TestCaveBuilder
{
    // ---- 生成内容の固定パラメータ ----
    public const string ChallengeCaveInfoFileName = "ch_ABEM_tutorial.txt"; // チャレンジ0面の洞窟定義ファイル名
    public const string UnitSpecFileName = "testmap_units.txt";             // caveinfo の {f008} が参照するユニット指定ファイル
    public const int FloorMax = 1;            // 階層数は1固定
    public const int RoomCount = 3;           // 部屋の個数は3固定
    public const float FloorTimeSeconds = 500f; // 洞窟の時間は500秒固定
    public const int DefaultPikminPerColor = 10; // ピクミン既定値は各種10匹ずつ
    public const int DefaultSprayPerType = 2;    // Spray既定値は各種2個ずつ
    public const string FixedBgmFile = "new_04_0.cnd"; // BGMは1つ固定(soil系，0面で実績のある曲)

    // 固定で登録するユニット(通路 / 行き止まり / 部屋)．編集中ユニットは実行時に追加する
    public static readonly string[] FixedUnitNames = { "wayl_tsuchi", "cap_tsuchi", "room_4x4a_4_conc" };

    private static readonly Encoding ShiftJis = CreateShiftJis();

    //-------------------------------------------------------------------------------
    // Shift_JIS エンコーディングを取得する処理(チャレンジ系テキストの文字化け防止)
    //-------------------------------------------------------------------------------
    private static Encoding CreateShiftJis()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding("shift_jis");
    }

    //-------------------------------------------------------------------------------
    // テスト用洞窟を生成してチャレンジ0面へ反映する処理(メインエントリ)
    //-------------------------------------------------------------------------------
    public static TestCaveBuildResult Build(string caveUnitsRoot, string allUnitsPath, string currentUnitName, TestCaveBuildOptions options)
    {
        return Build(caveUnitsRoot, allUnitsPath, currentUnitName, options, false);
    }

    //-------------------------------------------------------------------------------
    // テスト用洞窟を生成してチャレンジ0面へ反映する処理(表示言語指定付き)
    //-------------------------------------------------------------------------------
    public static TestCaveBuildResult Build(string caveUnitsRoot, string allUnitsPath, string currentUnitName, TestCaveBuildOptions options, bool english)
    {
        try
        {
            // ---- 入力検証 ----
            if (string.IsNullOrWhiteSpace(caveUnitsRoot) || !Directory.Exists(caveUnitsRoot))
            {
                return Fail(Text("MissingUnitsRoot", english));
            }

            if (string.IsNullOrWhiteSpace(allUnitsPath) || !File.Exists(allUnitsPath))
            {
                return Fail(Text("MissingAllUnits", english));
            }

            if (string.IsNullOrWhiteSpace(currentUnitName))
            {
                return Fail(Text("MissingCurrentUnit", english));
            }

            // ---- 出力先パスの解決 ----
            string? userDir = FindAncestorNamed(caveUnitsRoot, "user");
            if (userDir is null)
            {
                return Fail(Text("MissingUserDirectory", english));
            }

            string mapunitsDir = Directory.GetParent(caveUnitsRoot)?.FullName
                ?? throw new DirectoryNotFoundException(Text("MissingMapunitsDirectory", english));
            string caveinfoDir = Path.Combine(mapunitsDir, "caveinfo");
            string? stagesPath = FindChallengeStagesFile(userDir);
            if (string.IsNullOrWhiteSpace(stagesPath))
            {
                return Fail(Text("MissingChallengeStages", english));
            }

            string? bgmPath = FindChallengeBgmListFile(userDir);
            if (string.IsNullOrWhiteSpace(bgmPath))
            {
                return Fail(Text("MissingChallengeBgm", english));
            }

            // ---- 登録するユニット一覧を確定(固定3種 + 編集中ユニット，重複排除) ----
            List<string> targetNames = new(FixedUnitNames);
            if (!targetNames.Contains(currentUnitName, StringComparer.OrdinalIgnoreCase))
            {
                targetNames.Add(currentUnitName);
            }

            // all_units.txt から各ユニットの定義ブロックをそのまま抜き出す(door links 等を保持)
            Dictionary<string, List<string>> unitBlocks = ExtractUnitBlocks(File.ReadAllLines(allUnitsPath, ShiftJis));
            List<(string Name, List<string> Block)> selectedUnits = new();
            List<string> missing = new();
            foreach (string name in targetNames)
            {
                if (unitBlocks.TryGetValue(name, out List<string>? block))
                {
                    selectedUnits.Add((name, block));
                }
                else
                {
                    missing.Add(name);
                }
            }

            if (missing.Count > 0)
            {
                return Fail(string.Format(CultureInfo.InvariantCulture, Text("MissingUnitDefinitions", english), string.Join(", ", missing)));
            }

            // ---- 各ファイルを生成(書き出し前に既存ファイルをバックアップ) ----
            Directory.CreateDirectory(caveinfoDir);

            List<string> written = new();

            // ユニット指定ファイル(caveinfo の {f008} から参照される)
            string unitSpecPath = Path.Combine(caveUnitsRoot, UnitSpecFileName);
            BackupIfNeeded(unitSpecPath);
            File.WriteAllText(unitSpecPath, BuildUnitSpecText(selectedUnits), ShiftJis);
            written.Add(unitSpecPath);

            // caveinfo (1 階層，部屋数 3，編集中ユニットを含むユニット指定を参照)
            string caveinfoPath = Path.Combine(caveinfoDir, ChallengeCaveInfoFileName);
            BackupIfNeeded(caveinfoPath);
            File.WriteAllText(caveinfoPath, BuildCaveInfoText(UnitSpecFileName), ShiftJis);
            written.Add(caveinfoPath);

            // stages.txt (テンプレの 0 面ブロックのみカスタマイズ)
            BackupIfNeeded(stagesPath);
            string stagesText = File.ReadAllText(stagesPath, ShiftJis);
            File.WriteAllText(stagesPath, UpdateChallengeStagesText(stagesText, options, english), ShiftJis);
            written.Add(stagesPath);

            // ChallengeBgmList.txt (既存ファイルの 0 面シーンだけを 1 階層 1 曲へ更新)
            BackupIfNeeded(bgmPath);
            string bgmText = File.ReadAllText(bgmPath, ShiftJis);
            File.WriteAllText(bgmPath, UpdateChallengeBgmListText(bgmText, english), ShiftJis);
            written.Add(bgmPath);

            string unitList = string.Join(", ", selectedUnits.Select(unit => unit.Name));
            return new TestCaveBuildResult(true,
                string.Format(CultureInfo.InvariantCulture, Text("BuildSuccess", english), unitList),
                written);
        }
        catch (Exception ex)
        {
            return Fail(string.Format(CultureInfo.InvariantCulture, Text("BuildFailed", english), ex.Message));
        }
    }

    //-------------------------------------------------------------------------------
    // テスト洞窟作成処理の表示文言を取得する処理
    //-------------------------------------------------------------------------------
    private static string Text(string key, bool english)
    {
        return key switch
        {
            "MissingUnitsRoot" => english
                ? "The cave unit folder (units) was not found. Load a cave unit in Cave mode before running this."
                : "洞窟ユニットフォルダ(units)が見つかりません．洞窟モードでユニットを読み込んでから実行してください．",
            "MissingAllUnits" => english ? "all_units.txt was not found." : "all_units.txt が見つかりません．",
            "MissingCurrentUnit" => english ? "No edited map unit is selected." : "編集中のマップユニットが選択されていません．",
            "MissingUserDirectory" => english
                ? "Could not identify the disc user folder. Set the reference target to extracted disc data."
                : "ディスクの user フォルダを特定できませんでした．ディスク抽出データを参照先に指定してください．",
            "MissingMapunitsDirectory" => english ? "Could not identify the mapunits folder." : "mapunits フォルダを特定できません．",
            "MissingChallengeStages" => english
                ? "Challenge stages.txt was not found.\nA stages.txt under user that references ch_ABEM_tutorial.txt is required."
                : "チャレンジ用 stages.txt が見つかりません．\nuser 配下に ch_ABEM_tutorial.txt を参照している stages.txt が必要です．",
            "MissingChallengeBgm" => english
                ? "ChallengeBgmList.txt was not found.\nCheck user\\Totaka\\ChallengeBgmList.txt."
                : "チャレンジ用 ChallengeBgmList.txt が見つかりません．\nuser\\Totaka\\ChallengeBgmList.txt を確認してください．",
            "MissingUnitDefinitions" => english
                ? "The following units were not found in all_units.txt:\n{0}"
                : "all_units.txt に次のユニットが見つかりません．\n{0}",
            "BuildSuccess" => english
                ? "Applied the test cave to challenge stage 0 (ch_ABEM_tutorial).\nRegistered units: {0}"
                : "テスト用洞窟をチャレンジ0面(ch_ABEM_tutorial)へ反映しました．\n登録ユニット: {0}",
            "BuildFailed" => english
                ? "Failed to create the test cave.\n{0}"
                : "テスト用洞窟の生成に失敗しました．\n{0}",
            "MissingStageBlocks" => english ? "No stage blocks were found in stages.txt." : "stages.txt 内にステージブロックが見つかりません．",
            "MissingChallengeStageBlock" => english
                ? $"No block for {ChallengeCaveInfoFileName} was found in stages.txt."
                : $"stages.txt 内に {ChallengeCaveInfoFileName} のブロックが見つかりません．",
            "MissingBgmBlocks" => english ? "No scene blocks were found in ChallengeBgmList.txt." : "ChallengeBgmList.txt 内にシーンブロックが見つかりません．",
            _ => key
        };
    }

    //-------------------------------------------------------------------------------
    // 失敗結果を生成するヘルパー
    //-------------------------------------------------------------------------------
    private static TestCaveBuildResult Fail(string message)
    {
        return new TestCaveBuildResult(false, message, Array.Empty<string>());
    }

    //-------------------------------------------------------------------------------
    // 指定パスから上位へ遡り，指定名のフォルダを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindAncestorNamed(string startDirectory, string targetName)
    {
        DirectoryInfo? directory = new(startDirectory);
        while (directory is not null)
        {
            if (string.Equals(directory.Name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 既存ファイルがあり .bak が無い場合のみ，元データを .bak へ退避する処理
    //-------------------------------------------------------------------------------
    private static void BackupIfNeeded(string path)
    {
        string backupPath = path + ".bak";
        if (File.Exists(path) && !File.Exists(backupPath))
        {
            File.Copy(path, backupPath);
        }
    }

    //-------------------------------------------------------------------------------
    // user 配下からチャレンジ用 stages.txt を探す処理
    //-------------------------------------------------------------------------------
    private static string? FindChallengeStagesFile(string userDir)
    {
        string directCandidate = Path.Combine(userDir, "Matoba", "challenge", "stages.txt");
        if (File.Exists(directCandidate) && LooksLikeChallengeStagesFile(directCandidate))
        {
            return directCandidate;
        }

        foreach (string candidate in Directory.EnumerateFiles(userDir, "stages.txt", SearchOption.AllDirectories))
        {
            if (LooksLikeChallengeStagesFile(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // user 配下からチャレンジ用 ChallengeBgmList.txt を探す処理
    //-------------------------------------------------------------------------------
    private static string? FindChallengeBgmListFile(string userDir)
    {
        string directCandidate = Path.Combine(userDir, "Totaka", "ChallengeBgmList.txt");
        if (File.Exists(directCandidate))
        {
            return directCandidate;
        }

        return Directory.EnumerateFiles(userDir, "ChallengeBgmList.txt", SearchOption.AllDirectories)
            .FirstOrDefault();
    }

    //-------------------------------------------------------------------------------
    // 指定ファイルがチャレンジ用 stages.txt かを判定する処理
    //-------------------------------------------------------------------------------
    private static bool LooksLikeChallengeStagesFile(string path)
    {
        try
        {
            string text = File.ReadAllText(path, ShiftJis);
            return text.Contains(ChallengeCaveInfoFileName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------------
    // all_units.txt をユニット名 → 定義ブロック({ ... })の辞書へ分解する処理
    // ブロックは次のユニット見出し(行頭 #)または } で区切る．
    // 閉じ } が欠けた不正データでも次の見出しで区切り，末尾に } を補う．
    //-------------------------------------------------------------------------------
    private static Dictionary<string, List<string>> ExtractUnitBlocks(string[] lines)
    {
        Dictionary<string, List<string>> blocks = new(StringComparer.OrdinalIgnoreCase);
        int index = 0;
        while (index < lines.Length)
        {
            if (lines[index].Trim() != "{")
            {
                index++;
                continue;
            }

            List<string> body = new() { lines[index] };
            string? folderName = null;
            int cursor = index + 1;
            bool closed = false;
            while (cursor < lines.Length)
            {
                string trimmed = lines[cursor].Trim();
                if (trimmed == "}")
                {
                    body.Add(lines[cursor]);
                    cursor++;
                    closed = true;
                    break;
                }

                if (trimmed.StartsWith('#'))
                {
                    break;                                  // 次ユニットの見出し → ここでブロック終端
                }

                if (folderName is null && lines[cursor].Contains("# foldername", StringComparison.Ordinal))
                {
                    folderName = lines[cursor].Split('#')[0].Trim();
                }

                body.Add(lines[cursor]);
                cursor++;
            }

            if (!closed)
            {
                body.Add("}");                              // 閉じ } 欠落の補完
            }

            if (!string.IsNullOrEmpty(folderName) && !blocks.ContainsKey(folderName))
            {
                blocks[folderName] = body;
            }

            index = cursor;
        }

        return blocks;
    }

    //-------------------------------------------------------------------------------
    // 抜き出したユニットブロックから units 指定ファイル本文を組み立てる処理
    //-------------------------------------------------------------------------------
    private static string BuildUnitSpecText(IReadOnlyList<(string Name, List<string> Block)> units)
    {
        StringBuilder sb = new();
        void L(string line) => sb.Append(line).Append("\r\n");

        L("#");
        L("#");
        L("#\tunits definition file");
        L("#");
        L("#");
        L($"{units.Count} \t# number of units");
        foreach ((string name, List<string> block) in units)
        {
            L($"# {name}");
            foreach (string line in block)
            {
                L(line);
            }
        }

        return sb.ToString();
    }

    //-------------------------------------------------------------------------------
    // 1 階層 / 部屋数 3 / 指定ユニットファイル参照の caveinfo 本文を組み立てる処理
    //-------------------------------------------------------------------------------
    private static string BuildCaveInfoText(string unitFile)
    {
        StringBuilder sb = new();
        void L(string line) => sb.Append(line).Append("\r\n");

        L("# CaveInfo");
        L("{");
        L($"\t{{c000}} 4 {FloorMax} \t# floor max");                  // 階層数
        L("\t{_eof} ");
        L("}");
        L($"{FloorMax} # FloorInfo");
        L("{");
        L("\t{f000} 4 0 \t# floor index start");                      // この FloorInfo の適用開始階
        L("\t{f001} 4 0 \t# floor index end");                        // 適用終了階(最終階 = 脱出間欠泉設置対象)
        L("\t{f002} 4 0 \t# teki max");                               // 敵最大数(テスト用は 0)
        L("\t{f003} 4 0 \t# item max");                               // アイテム最大数
        L("\t{f004} 4 0 \t# gate max");                               // ゲート最大数
        L("\t{f014} 4 0 \t# cap max");                                // キャップ最大数
        L($"\t{{f005}} 4 {RoomCount} \t# room count");                // 部屋数(3 固定)
        L("\t{f006} 4 0.000000 \t# route ratio");                     // 通路の割合
        L("\t{f007} 4 1 \t# escape fountain");                        // 最終階に脱出間欠泉を設置
        L($"\t{{f008}} -1 {unitFile} \t# cave unit file");            // 使用ユニット指定ファイル
        L("\t{f009} -1 normal_light_cha.ini \t# lighting");           // 使用ライト
        L("\t{f00A} -1 none \t# vrbox");
        L("\t{f010} 4 0 \t# hole clogged");
        L("\t{f011} 4 0 \t# floor alpha type");
        L("\t{f012} 4 0 \t# floor beta type");
        L("\t{f013} 4 0 \t# floor hidden");
        L("\t{f015} 4 1 \t# version");                                // version 1(CapInfo を読むために必須)
        L("\t{f016} 4 0.000000 \t# waterwraith timer");
        L("\t{f017} 4 0 \t# glitchy seesaw");
        L("\t{_eof} ");
        L("}");
        // 1 階層分の Teki / Item / Gate / Cap はすべて空
        L("# TekiInfo");
        L("{");
        L("\t0 \t# num");
        L("}");
        L("# ItemInfo");
        L("{");
        L("\t0 \t# num");
        L("}");
        L("# GateInfo");
        L("{");
        L("\t0 \t# num");
        L("}");
        L("# CapInfo");
        L("{");
        L("\t0 \t# num");
        L("}");

        return sb.ToString();
    }

    //-------------------------------------------------------------------------------
    // 既存の stages.txt からチャレンジ 0 面ブロックだけを更新する処理
    //-------------------------------------------------------------------------------
    private static string UpdateChallengeStagesText(string existingText, TestCaveBuildOptions options, bool english)
    {
        string newLine = DetectNewLine(existingText);
        List<string> lines = SplitLines(existingText);
        List<(int Start, int End)> blocks = GetTopLevelBlocks(lines);
        if (blocks.Count == 0)
        {
            throw new InvalidDataException(Text("MissingStageBlocks", english));
        }

        int blockIndex = FindChallengeStageBlockIndex(lines, blocks);
        if (blockIndex < 0)
        {
            throw new InvalidDataException(Text("MissingChallengeStageBlock", english));
        }

        (int Start, int End) targetBlock = blocks[blockIndex];
        List<string> updatedBlock = BuildUpdatedChallengeStageBlock(lines, targetBlock.Start, targetBlock.End, options);
        lines.RemoveRange(targetBlock.Start, targetBlock.End - targetBlock.Start + 1);
        lines.InsertRange(targetBlock.Start, updatedBlock);
        return string.Join(newLine, lines);
    }

    //-------------------------------------------------------------------------------
    // 既存の ChallengeBgmList.txt から 0 面シーンだけを更新する処理
    //-------------------------------------------------------------------------------
    private static string UpdateChallengeBgmListText(string existingText, bool english)
    {
        string newLine = DetectNewLine(existingText);
        List<string> lines = SplitLines(existingText);
        List<(int Start, int End)> blocks = GetTopLevelBlocks(lines);
        if (blocks.Count == 0)
        {
            throw new InvalidDataException(Text("MissingBgmBlocks", english));
        }

        (int Start, int End) targetBlock = blocks[0];
        List<string> updatedBlock = BuildUpdatedChallengeBgmBlock(lines, targetBlock.Start, targetBlock.End);
        lines.RemoveRange(targetBlock.Start, targetBlock.End - targetBlock.Start + 1);
        lines.InsertRange(targetBlock.Start, updatedBlock);
        return string.Join(newLine, lines);
    }

    //-------------------------------------------------------------------------------
    // stages.txt の対象ブロックを既存コメントを残しつつ更新する処理
    //-------------------------------------------------------------------------------
    private static List<string> BuildUpdatedChallengeStageBlock(IReadOnlyList<string> lines, int start, int end, TestCaveBuildOptions options)
    {
        List<string> updated = new();
        bool skipFloorTimeLines = false;
        int dataLineIndex = 0;

        for (int index = start; index <= end; index++)
        {
            string line = lines[index];
            string trimmed = line.Trim();

            if (trimmed == "{")
            {
                updated.Add(line);
                continue;
            }

            if (trimmed == "}")
            {
                updated.Add(line);
                continue;
            }

            if (skipFloorTimeLines)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                updated.Add(line);
                continue;
            }

            if (dataLineIndex == 0)
            {
                updated.Add(line); // version は既存値を維持
                dataLineIndex++;
                continue;
            }

            if (dataLineIndex == 1)
            {
                updated.Add($"{GetIndentation(line)}{ChallengeCaveInfoFileName} ");
                dataLineIndex++;
                continue;
            }

            string comment = ExtractComment(line);
            Match piki = Regex.Match(comment, @"col(\d)\s+happa(\d)");
            if (piki.Success)
            {
                int color = int.Parse(piki.Groups[1].Value, CultureInfo.InvariantCulture);
                int happa = int.Parse(piki.Groups[2].Value, CultureInfo.InvariantCulture);
                int value = GetChallengePikminCount(color, happa, options);
                updated.Add(BuildValueLine(line, value, comment));
            }
            else if (comment.Contains("dope black", StringComparison.Ordinal))
            {
                updated.Add(BuildValueLine(line, options.BitterSprayCount, comment));
            }
            else if (comment.Contains("dope red", StringComparison.Ordinal))
            {
                updated.Add(BuildValueLine(line, options.SpicySprayCount, comment));
            }
            else if (comment.Contains("# time", StringComparison.Ordinal))
            {
                updated.Add(BuildValueLine(line, FloorTimeSeconds.ToString("F6", CultureInfo.InvariantCulture), comment));
            }
            else if (comment.Contains("floor num", StringComparison.Ordinal))
            {
                updated.Add(BuildValueLine(line, FloorMax, comment));
            }
            else if (comment.Contains("otakara num", StringComparison.Ordinal))
            {
                updated.Add(BuildValueLine(line, 0, comment));
            }
            else if (comment.Contains("2d index", StringComparison.Ordinal))
            {
                updated.Add(line);
                string floorTimeComment = ExtractFirstTrailingComment(lines, index + 1, end) ?? "# floor 0 time";
                updated.Add(BuildValueLine(line, FloorTimeSeconds.ToString("F6", CultureInfo.InvariantCulture), floorTimeComment));
                skipFloorTimeLines = true;
            }
            else
            {
                updated.Add(line);
            }

            dataLineIndex++;
        }

        return updated;
    }

    //-------------------------------------------------------------------------------
    // challenge 用 PikiCounter の値を種別ごとに返す処理
    //-------------------------------------------------------------------------------
    private static int GetChallengePikminCount(int color, int happa, TestCaveBuildOptions options)
    {
        if (happa != 0)
        {
            return 0;
        }

        return color switch
        {
            0 => options.BluePikminCount,
            1 => options.RedPikminCount,
            2 => options.YellowPikminCount,
            3 => options.PurplePikminCount,
            4 => options.WhitePikminCount,
            5 => options.BulbminCount,
            6 => options.CarrotCount,
            _ => 0
        };
    }

    //-------------------------------------------------------------------------------
    // ChallengeBgmList.txt の 0 面シーンブロックを既存コメントを残しつつ更新する処理
    //-------------------------------------------------------------------------------
    private static List<string> BuildUpdatedChallengeBgmBlock(IReadOnlyList<string> lines, int start, int end)
    {
        List<string> updated = new();
        int dataLineIndex = 0;

        for (int index = start; index <= end; index++)
        {
            string line = lines[index];
            string trimmed = line.Trim();

            if (trimmed == "{")
            {
                updated.Add(line);
                continue;
            }

            if (trimmed == "}")
            {
                updated.Add(line);
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                updated.Add(line);
                continue;
            }

            if (dataLineIndex == 0)
            {
                updated.Add(BuildValueLine(line, FloorMax, ExtractComment(line)));
            }
            else if (dataLineIndex == 1)
            {
                updated.Add(BuildValueLine(line, FixedBgmFile, ExtractComment(line)));
            }

            dataLineIndex++;
        }

        return updated;
    }

    //-------------------------------------------------------------------------------
    // 改行コードを維持するために，入力文字列の改行種別を判定する処理
    //-------------------------------------------------------------------------------
    private static string DetectNewLine(string text)
    {
        return text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    }

    //-------------------------------------------------------------------------------
    // 文字列を行配列へ分解する処理
    //-------------------------------------------------------------------------------
    private static List<string> SplitLines(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
    }

    //-------------------------------------------------------------------------------
    // 行配列からトップレベルの { } ブロック範囲を抽出する処理
    //-------------------------------------------------------------------------------
    private static List<(int Start, int End)> GetTopLevelBlocks(IReadOnlyList<string> lines)
    {
        List<(int Start, int End)> blocks = new();
        int start = -1;
        for (int index = 0; index < lines.Count; index++)
        {
            string trimmed = lines[index].Trim();
            if (trimmed == "{")
            {
                start = index;
            }
            else if (trimmed == "}" && start >= 0)
            {
                blocks.Add((start, index));
                start = -1;
            }
        }

        return blocks;
    }

    //-------------------------------------------------------------------------------
    // stages.txt 内で ch_ABEM_tutorial.txt を参照しているブロックを探す処理
    //-------------------------------------------------------------------------------
    private static int FindChallengeStageBlockIndex(IReadOnlyList<string> lines, IReadOnlyList<(int Start, int End)> blocks)
    {
        for (int blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
        {
            (int start, int end) = blocks[blockIndex];
            string? stageName = ExtractStageName(lines, start, end);
            if (string.Equals(stageName, ChallengeCaveInfoFileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(stageName, Path.GetFileNameWithoutExtension(ChallengeCaveInfoFileName), StringComparison.OrdinalIgnoreCase))
            {
                return blockIndex;
            }
        }

        return -1;
    }

    //-------------------------------------------------------------------------------
    // ステージブロックから参照している洞窟ファイル名を取り出す処理
    //-------------------------------------------------------------------------------
    private static string? ExtractStageName(IReadOnlyList<string> lines, int start, int end)
    {
        int dataLineIndex = 0;
        for (int index = start; index <= end; index++)
        {
            string trimmed = lines[index].Trim();
            if (trimmed is "{" or "}" || string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (dataLineIndex == 1)
            {
                return trimmed.Split('#')[0].Trim();
            }

            dataLineIndex++;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 対象範囲の後続行から，最初に見つかったコメントを取り出す処理
    //-------------------------------------------------------------------------------
    private static string? ExtractFirstTrailingComment(IReadOnlyList<string> lines, int start, int end)
    {
        for (int index = start; index <= end; index++)
        {
            string comment = ExtractComment(lines[index]);
            if (!string.IsNullOrWhiteSpace(comment))
            {
                return comment;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 元行のインデントとコメントを維持した値行を組み立てる処理
    //-------------------------------------------------------------------------------
    private static string BuildValueLine(string sourceLine, object value, string comment)
    {
        string indent = GetIndentation(sourceLine);
        if (string.IsNullOrWhiteSpace(comment))
        {
            return $"{indent}{value}";
        }

        return $"{indent}{value} \t{comment}";
    }

    //-------------------------------------------------------------------------------
    // 行頭インデント文字列を取得する処理
    //-------------------------------------------------------------------------------
    private static string GetIndentation(string line)
    {
        int length = 0;
        while (length < line.Length && char.IsWhiteSpace(line[length]))
        {
            length++;
        }

        return line[..length];
    }

    //-------------------------------------------------------------------------------
    // 「<TAB>値 <TAB>コメント」形式の 1 行を追加する処理(テンプレのコメントを維持)
    //-------------------------------------------------------------------------------
    private static void AppendValueLine(StringBuilder sb, object value, string comment)
    {
        sb.Append('\t').Append(value).Append(" \t").Append(comment).Append("\r\n");
    }

    //-------------------------------------------------------------------------------
    // 行から # 以降のコメント部分を取り出す処理(# が無ければ空文字)
    //-------------------------------------------------------------------------------
    private static string ExtractComment(string line)
    {
        int index = line.IndexOf('#');
        return index >= 0 ? line[index..] : string.Empty;
    }
}
