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
        ["Water Drain"] = "WaterDrains_icon.png",
        ["Teki: Pellet Posy"] = "Teki_PelletPosy_icon.png",
        ["Teki: Honeywisp"] = "Teki_Honeywisp_icon.png",
        ["Pellet / Treasure"] = "02_Item.png",
        ["Pellet"] = "02_Item.png",
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
        string? fileName = ResolveIconFileName(label);
        if (fileName is null)
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
            "Short Bridge" => new SizeF(130f, 260f),
            "Short Bridge (Slanted)" => new SizeF(130f, 200f),
            "Long Bridge" => new SizeF(130f, 440f),
            "Gate" => new SizeF(267f, 70f),
            "Electric Gate" => new SizeF(267f, 35f),
            "Small Block" or "Small Block [Seesaw]" => new SizeF(100f, 100f),
            "Normal Block" or "Normal Block [Seesaw]" => new SizeF(150f, 120f),
            "Paper Bag" => new SizeF(256f, 197f),
            _ => null
        };
    }

    //-------------------------------------------------------------------------------
    // 地上 object の表示名から参考元と同じ前方オフセットを取得する処理
    //-------------------------------------------------------------------------------
    public static PointF GetFootprintOffset(string label)
    {
        return label switch
        {
            "Short Bridge" => new PointF(0f, 88f),
            "Short Bridge (Slanted)" => new PointF(0f, 58f),
            "Long Bridge" => new PointF(0f, 178f),
            "Electric Gate" => new PointF(0f, -10f),
            _ => PointF.Empty
        };
    }

    //-------------------------------------------------------------------------------
    // 地上 object の表示名からアイコンファイル名を解決する処理
    //-------------------------------------------------------------------------------
    private static string? ResolveIconFileName(string label)
    {
        if (FileNames.TryGetValue(label, out string? fileName))
        {
            return fileName;
        }

        if (label.StartsWith("Treasure:", StringComparison.OrdinalIgnoreCase) ||
            label.StartsWith("ExpKit Treasure:", StringComparison.OrdinalIgnoreCase) ||
            label.StartsWith("Unknown treasure:", StringComparison.OrdinalIgnoreCase) ||
            label.StartsWith("Unknown exploration kit treasure:", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Pellet", StringComparison.OrdinalIgnoreCase))
        {
            return "02_Item.png";
        }

        if (label.StartsWith("Burg. Spiderwort", StringComparison.OrdinalIgnoreCase))
        {
            return "06_Plant.png";
        }

        if (TryResolveTekiIconFileName(label, out string? tekiFileName))
        {
            return tekiFileName;
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // Teki 表示名から専用アイコンファイル名を解決する処理
    //-------------------------------------------------------------------------------
    private static bool TryResolveTekiIconFileName(string label, out string? fileName)
    {
        fileName = null;
        const string prefix = "Teki: ";
        if (!label.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string name = label[prefix.Length..];
        if (IsPlantTekiName(name))
        {
            fileName = "Teki_plant_icon.png";
            return true;
        }

        if (name.Equals("Bomb rock", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Falling boulder", StringComparison.OrdinalIgnoreCase))
        {
            fileName = "Teki_Stone_icon.png";
            return true;
        }

        if (name.Equals("Egg", StringComparison.OrdinalIgnoreCase))
        {
            fileName = "Teki_egg_icon.png";
            return true;
        }

        return false;
    }

    //-------------------------------------------------------------------------------
    // 草系として扱う Teki 名かどうかを判定する処理
    //-------------------------------------------------------------------------------
    private static bool IsPlantTekiName(string name)
    {
        return name.Equals("Dandelion", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Seeding Dandelion", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Clover", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Figwort", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Horsetail", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Glowstem", StringComparison.OrdinalIgnoreCase);
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
