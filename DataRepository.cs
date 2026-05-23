namespace PikminUnitEditor;

internal sealed class DataRepository
{
    private readonly string _baseDirectory;

    public DataRepository(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public string? TryFindDefaultToolkitPath()
    {
        return FindUpwardFile("Hocotate_Toolkit_v1.23a", "Hocotate_Toolkit.exe");
    }

    public string? TryFindDefaultCaveInfoPath()
    {
        return FindUpwardFile("検証用メインデータ", "caveinfo.txt");
    }

    public string? TryFindDefaultUnitsPath()
    {
        string? allUnits = FindUpwardFile("検証用メインデータ", "all_units.txt");
        if (allUnits is not null)
        {
            return Path.GetDirectoryName(allUnits);
        }

        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "root", "user", "Mukki", "mapunits", "units");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 展開済み root から洞窟 arc フォルダを探す処理
    //-------------------------------------------------------------------------------
    public string? TryFindDefaultCaveArcRoot()
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "root", "user", "Mukki", "mapunits", "arc");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    public string? TryFindDefaultDiscRootPath()
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "検証用メインデータ");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    public string? TryFindDefaultLocationRoot()
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "pikmingen_editor", "tools", "Location");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 展開済み root から地上マップフォルダを探す処理
    //-------------------------------------------------------------------------------
    public string? TryFindDefaultFieldMapRoot()
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "root", "user", "Abe", "map");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 展開済み root から地上 texts アーカイブフォルダを探す処理
    //-------------------------------------------------------------------------------
    public string? TryFindDefaultFieldTextsRoot()
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, "root", "user", "Kando", "map");
            if (Directory.Exists(candidateRoot))
            {
                return candidateRoot;
            }
        }

        return null;
    }

    public IReadOnlyDictionary<string, string> LoadUnitImageCatalog()
    {
        string? csvPath = FindUpwardFile("検証用メインデータ", "Unit_Category_1.csv");
        if (csvPath is null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        string imageRoot = Path.GetDirectoryName(csvPath)!;
        Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (string line in File.ReadLines(csvPath).Skip(1))
        {
            string[] cols = line.Split(',');
            if (cols.Length < 6)
            {
                continue;
            }

            string unitName = cols[0].Trim();
            string imageName = cols[5].Trim();
            string imagePath = Path.Combine(imageRoot, imageName);

            if (!string.IsNullOrWhiteSpace(unitName) && File.Exists(imagePath))
            {
                map[unitName] = imagePath;
            }
        }

        return map;
    }

    public IReadOnlyDictionary<string, int> LoadUnitImageRotationOverrides()
    {
        string? csvPath = FindUpwardFile("検証用メインデータ", "Unit_Image_Rotation.csv");
        if (csvPath is null)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        Dictionary<string, int> map = new(StringComparer.OrdinalIgnoreCase);
        foreach (string rawLine in File.ReadLines(csvPath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            string[] cols = line.Split(',');
            if (cols.Length < 2)
            {
                continue;
            }

            string unitName = cols[0].Trim();
            if (string.IsNullOrWhiteSpace(unitName))
            {
                continue;
            }

            if (int.TryParse(cols[1].Trim(), out int rotation))
            {
                map[unitName] = rotation;
            }
        }

        return map;
    }

    private string? FindUpwardFile(string childDirectory, string targetName)
    {
        DirectoryInfo? current = new(_baseDirectory);
        for (int i = 0; i < 6 && current is not null; i++, current = current.Parent)
        {
            string candidateRoot = Path.Combine(current.FullName, childDirectory);
            if (!Directory.Exists(candidateRoot))
            {
                continue;
            }

            string? dir = RecursiveFinder.FindDirectoryContainingFile(candidateRoot, targetName);
            if (dir is not null)
            {
                return Path.Combine(dir, targetName);
            }
        }

        return null;
    }
}
