namespace PikminUnitEditor;

internal static class FieldObjectIconCatalog
{
    private static readonly Dictionary<string, string> FileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bridge"] = "bridge.png",
        ["Short Bridge"] = "sbridge.png",
        ["Short Bridge (Slanted)"] = "ubridge.png",
        ["Long Bridge"] = "lbridge.png",
        ["Gate"] = "gate.png",
        ["Electric Gate"] = "dgat.png",
        ["Small Block"] = "downfloor1.png",
        ["Normal Block"] = "downfloor2.png",
        ["Small Block [Seesaw]"] = "downfloor1.png",
        ["Normal Block [Seesaw]"] = "downfloor2.png",
        ["Down Floor / Paper Bag"] = "downfloor1.png",
        ["Paper Bag"] = "paperbag.png",
        ["Blue Onion"] = "Blue_Onion.png",
        ["Red Onion"] = "Red_Onion.png",
        ["Yellow Onion"] = "Yellow_Onion.png",
        ["Rocket"] = "Rocket.png",
        ["Onion / Rocket"] = "Rocket.png",
        ["Cave Entrance"] = "05_Hole_Geyser.png",
        ["Item"] = "02_Item.png",
        ["Pikmin"] = "Pikmin_icon.png"
    };

    private static readonly Dictionary<string, Image?> Icons = new(StringComparer.OrdinalIgnoreCase);
    private static string? s_resourceDirectory;

    //-------------------------------------------------------------------------------
    // 地上 object の表示名からアイコン画像を取得する処理
    //-------------------------------------------------------------------------------
    public static Image? GetIcon(string label)
    {
        if (!FileNames.TryGetValue(label, out string? fileName))
        {
            return null;
        }

        if (Icons.TryGetValue(label, out Image? cachedIcon))
        {
            return cachedIcon;
        }

        Image? icon = EmbeddedImageCatalog.LoadImage("スポーンアイコン.resources", fileName) ??
            EmbeddedImageCatalog.LoadImage("スポーンアイコン", fileName);
        if (icon is null)
        {
            string? path = FindIconPath(fileName);
            icon = File.Exists(path) ? Image.FromFile(path) : null;
        }

        Icons[label] = icon;
        return icon;
    }

    //-------------------------------------------------------------------------------
    // 地上 object の表示名から X/Z フットプリントサイズを取得する処理
    //-------------------------------------------------------------------------------
    public static SizeF? GetFootprintSize(string label)
    {
        return label switch
        {
            "Short Bridge" => new SizeF(95f, 210f),
            "Short Bridge (Slanted)" => new SizeF(95f, 250f),
            "Long Bridge" => new SizeF(105f, 430f),
            "Gate" => new SizeF(210f, 70f),
            "Electric Gate" => new SizeF(210f, 70f),
            "Small Block" or "Small Block [Seesaw]" => new SizeF(95f, 95f),
            "Normal Block" or "Normal Block [Seesaw]" => new SizeF(130f, 130f),
            "Paper Bag" => new SizeF(160f, 95f),
            _ => null
        };
    }

    //-------------------------------------------------------------------------------
    // 地上 object の表示名からフットプリント色を取得する処理
    //-------------------------------------------------------------------------------
    public static Color GetFootprintColor(string label)
    {
        if (label.Contains("Bridge", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(64, 97, 74, 43);
        }

        if (label.Contains("Gate", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(72, 96, 92, 110);
        }

        if (label.Contains("Block", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Paper Bag", StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb(60, 116, 91, 52);
        }

        return Color.FromArgb(56, 70, 92, 120);
    }

    //-------------------------------------------------------------------------------
    // resources フォルダを取得する処理
    //-------------------------------------------------------------------------------
    private static string? FindResourceDirectory()
    {
        if (s_resourceDirectory is not null)
        {
            return s_resourceDirectory;
        }

        foreach (string baseDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            string? found = FindResourceDirectoryFrom(baseDirectory);
            if (found is not null)
            {
                s_resourceDirectory = found;
                return found;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // resources と通常スポーンアイコンから指定アイコンを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindIconPath(string fileName)
    {
        string? resourceDirectory = FindResourceDirectory();
        if (resourceDirectory is not null)
        {
            string resourceCandidate = Path.Combine(resourceDirectory, fileName);
            if (File.Exists(resourceCandidate))
            {
                return resourceCandidate;
            }

            string? iconDirectory = Directory.GetParent(resourceDirectory)?.FullName;
            if (iconDirectory is not null)
            {
                string iconCandidate = Path.Combine(iconDirectory, fileName);
                if (File.Exists(iconCandidate))
                {
                    return iconCandidate;
                }
            }
        }

        foreach (string baseDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            string? found = FindIconPathFrom(baseDirectory, fileName);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダから親階層へ向かってスポーンアイコンを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindIconPathFrom(string baseDirectory, string fileName)
    {
        DirectoryInfo? directory = new(baseDirectory);
        for (int depth = 0; directory is not null && depth < 8; depth++)
        {
            string resourcesCandidate = Path.Combine(directory.FullName, "スポーンアイコン", "resources", fileName);
            if (File.Exists(resourcesCandidate))
            {
                return resourcesCandidate;
            }

            string iconCandidate = Path.Combine(directory.FullName, "スポーンアイコン", fileName);
            if (File.Exists(iconCandidate))
            {
                return iconCandidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダから親階層へ向かって resources フォルダを探す処理
    //-------------------------------------------------------------------------------
    private static string? FindResourceDirectoryFrom(string baseDirectory)
    {
        DirectoryInfo? directory = new(baseDirectory);
        for (int depth = 0; directory is not null && depth < 8; depth++)
        {
            string candidate = Path.Combine(directory.FullName, "スポーンアイコン", "resources");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
