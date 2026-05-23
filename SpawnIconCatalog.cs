namespace PikminUnitEditor;

internal static class SpawnIconCatalog
{
    private static readonly Dictionary<int, string> FileNames = new()
    {
        [0] = "00_Teki_A.png",
        [1] = "01_Teki_B.png",
        [2] = "02_Item.png",
        [4] = "05_Hole_Geyser.png",
        [5] = "05_Hole_Geyser.png",
        [6] = "06_Plant.png",
        [7] = "07_Start.png",
        [8] = "08_Teki_F.png"
    };

    private static readonly Dictionary<int, Image?> Icons = new();
    private static string? s_iconDirectory;

    //-------------------------------------------------------------------------------
    // 指定したスポーン種別に対応するアイコン画像を取得する処理
    //-------------------------------------------------------------------------------
    public static Image? GetIcon(int typeId)
    {
        if (!FileNames.TryGetValue(typeId, out string? fileName))
        {
            return null;
        }

        if (Icons.TryGetValue(typeId, out Image? cachedIcon))
        {
            return cachedIcon;
        }

        Image? icon = EmbeddedImageCatalog.LoadImage("スポーンアイコン", fileName);
        if (icon is null)
        {
            string? directory = FindIconDirectory();
            string path = directory is null ? string.Empty : Path.Combine(directory, fileName);
            icon = File.Exists(path) ? Image.FromFile(path) : null;
        }

        Icons[typeId] = icon;
        return icon;
    }

    //-------------------------------------------------------------------------------
    // スポーンアイコンの格納フォルダを取得する処理
    //-------------------------------------------------------------------------------
    private static string? FindIconDirectory()
    {
        if (s_iconDirectory is not null)
        {
            return s_iconDirectory;
        }

        foreach (string baseDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            string? found = FindIconDirectoryFrom(baseDirectory);
            if (found is not null)
            {
                s_iconDirectory = found;
                return found;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダから親階層へ向かってスポーンアイコンフォルダを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindIconDirectoryFrom(string baseDirectory)
    {
        DirectoryInfo? directory = new(baseDirectory);
        for (int depth = 0; directory is not null && depth < 8; depth++)
        {
            string candidate = Path.Combine(directory.FullName, "スポーンアイコン");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
