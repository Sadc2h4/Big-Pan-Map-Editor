using System.Diagnostics;

namespace PikminUnitEditor;

internal sealed record FieldMapCacheEntry(
    string MapName,
    string CacheDirectory,
    string? ObjPath,
    string? MtlPath,
    string? PreviewImagePath,
    string? WaterboxPath);

internal sealed class FieldMapCacheService
{
    private readonly string _fieldMapRoot;
    private readonly string? _fieldTextsRoot;
    private readonly string? _toolkitPath;
    private readonly string _cacheRoot;
    private readonly string _fieldCacheDir;
    private readonly string _fieldImageCacheDir;

    public FieldMapCacheService(string fieldMapRoot, string? fieldTextsRoot, string? toolkitPath, string? cacheRootOverride)
    {
        _fieldMapRoot = fieldMapRoot;
        _fieldTextsRoot = fieldTextsRoot;
        _toolkitPath = File.Exists(toolkitPath) ? toolkitPath : null;
        _cacheRoot = !string.IsNullOrWhiteSpace(cacheRootOverride)
            ? cacheRootOverride
            : Path.GetDirectoryName(fieldMapRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? fieldMapRoot;
        _fieldCacheDir = Path.Combine(_cacheRoot, "地上キャッシュ");
        _fieldImageCacheDir = Path.Combine(_cacheRoot, "地上画像キャッシュ");
    }

    //-------------------------------------------------------------------------------
    // 指定地上マップの表示用キャッシュを用意する処理
    //-------------------------------------------------------------------------------
    public FieldMapCacheEntry EnsureFieldMapCache(string mapName, Action<string, int, int>? progressCallback = null)
    {
        progressCallback?.Invoke(mapName, 0, 4);
        Directory.CreateDirectory(_fieldCacheDir);
        Directory.CreateDirectory(_fieldImageCacheDir);

        string cacheDir = Path.Combine(_fieldCacheDir, mapName);
        string arcExtractDir = Path.Combine(cacheDir, "arc");
        string textsExtractDir = Path.Combine(cacheDir, "texts");
        string imagePath = Path.Combine(_fieldImageCacheDir, mapName + ".png");
        Directory.CreateDirectory(cacheDir);

        progressCallback?.Invoke(mapName, 1, 4);
        string? arcArchivePath = ResolveMapArchivePath(mapName, "arc.szs");
        if (arcArchivePath is not null)
        {
            EnsureArchiveExtracted(arcArchivePath, arcExtractDir);
        }

        progressCallback?.Invoke(mapName, 2, 4);
        string? textsArchivePath = ResolveMapArchivePath(mapName, "texts.szs");
        if (textsArchivePath is not null)
        {
            EnsureArchiveExtracted(textsArchivePath, textsExtractDir);
        }

        FieldModelPaths modelPaths = ResolveModelPaths(arcExtractDir);
        if ((modelPaths.ObjPath is null || modelPaths.MtlPath is null) && modelPaths.BmdPath is not null)
        {
            modelPaths = ConvertBmdModel(modelPaths.BmdPath, arcExtractDir);
        }

        progressCallback?.Invoke(mapName, 3, 4);
        if (ShouldRenderPreviewImage(imagePath, modelPaths.ObjPath, modelPaths.MtlPath, modelPaths.DaePath))
        {
            TryRenderPreviewImage(modelPaths.ObjPath, modelPaths.MtlPath, modelPaths.DaePath, imagePath);
        }

        string? waterboxPath = FindFirstExistingPathOrNull(
            Path.Combine(textsExtractDir, "waterbox.txt"),
            Directory.Exists(textsExtractDir)
                ? Directory.GetFiles(textsExtractDir, "waterbox.txt", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty
                : string.Empty);

        DeleteToolkitSidecarDirectory(cacheDir);
        progressCallback?.Invoke(mapName, 4, 4);

        return new FieldMapCacheEntry(
            mapName,
            cacheDir,
            modelPaths.ObjPath,
            modelPaths.MtlPath,
            File.Exists(imagePath) ? imagePath : null,
            waterboxPath);
    }

    //-------------------------------------------------------------------------------
    // 地上マップ名から Kando 側アーカイブパスを解決する処理
    //-------------------------------------------------------------------------------
    private string? ResolveMapArchivePath(string mapName, string archiveName)
    {
        if (string.IsNullOrWhiteSpace(_fieldTextsRoot))
        {
            return null;
        }

        string path = Path.Combine(_fieldTextsRoot, mapName, archiveName);
        return File.Exists(path) ? path : null;
    }

    //-------------------------------------------------------------------------------
    // 展開済み arc から利用可能なモデルファイルを探す処理
    //-------------------------------------------------------------------------------
    private static FieldModelPaths ResolveModelPaths(string arcExtractDir)
    {
        if (!Directory.Exists(arcExtractDir))
        {
            return new FieldModelPaths(null, null, null, null);
        }

        string? bmdPath = PickPreferredFile(Directory.GetFiles(arcExtractDir, "*.bmd", SearchOption.AllDirectories));
        FieldModelPaths convertedPaths = ResolveConvertedModelPaths(bmdPath);
        string? objPath = convertedPaths.ObjPath ?? (bmdPath is null
            ? PickPreferredFile(Directory.GetFiles(arcExtractDir, "*.obj", SearchOption.AllDirectories))
            : null);
        string? mtlPath = objPath is null
            ? PickPreferredFile(Directory.GetFiles(arcExtractDir, "*.mtl", SearchOption.AllDirectories))
            : FindFirstExistingPathOrNull(
                Path.ChangeExtension(objPath, ".mtl"),
                objPath + ".mtl",
                Path.Combine(Path.GetDirectoryName(objPath) ?? arcExtractDir, "view.obj.mtl"))
                ?? PickPreferredFile(Directory.GetFiles(arcExtractDir, "*.mtl", SearchOption.AllDirectories));
        string? daePath = convertedPaths.DaePath ?? (bmdPath is null
            ? PickPreferredFile(Directory.GetFiles(arcExtractDir, "*.dae", SearchOption.AllDirectories))
            : null);
        return new FieldModelPaths(objPath, mtlPath, daePath, bmdPath);
    }

    //-------------------------------------------------------------------------------
    // 選定した BMD 専用の変換済みモデルパスを取得する処理
    //-------------------------------------------------------------------------------
    private static FieldModelPaths ResolveConvertedModelPaths(string? bmdPath)
    {
        if (string.IsNullOrWhiteSpace(bmdPath))
        {
            return new FieldModelPaths(null, null, null, null);
        }

        string modelName = Path.GetFileNameWithoutExtension(bmdPath);
        string conversionDir = Path.Combine(Path.GetDirectoryName(bmdPath) ?? string.Empty, modelName + "_view");
        string objPath = Path.Combine(conversionDir, "view.obj");
        string mtlPath = Path.Combine(conversionDir, "view.obj.mtl");
        string daePath = Path.Combine(conversionDir, "view.dae");
        return new FieldModelPaths(
            File.Exists(objPath) ? objPath : null,
            File.Exists(mtlPath) ? mtlPath : null,
            File.Exists(daePath) ? daePath : null,
            bmdPath);
    }

    //-------------------------------------------------------------------------------
    // BMD を OBJ/DAE に変換してモデルパスを取得する処理
    //-------------------------------------------------------------------------------
    private FieldModelPaths ConvertBmdModel(string bmdPath, string arcExtractDir)
    {
        string modelName = Path.GetFileNameWithoutExtension(bmdPath);
        string conversionDir = Path.Combine(Path.GetDirectoryName(bmdPath) ?? arcExtractDir, modelName + "_view");
        Directory.CreateDirectory(conversionDir);
        string objPath = Path.Combine(conversionDir, "view.obj");
        string daePath = Path.Combine(conversionDir, "view.dae");

        if (!File.Exists(objPath))
        {
            TryRunToolkit($"--bmd2obj \"{bmdPath}\" \"{objPath}\"", conversionDir);
        }

        if (!File.Exists(daePath))
        {
            TryRunToolkit($"--bmd2dae \"{bmdPath}\" \"{daePath}\"", conversionDir);
        }

        DeleteToolkitSidecarDirectory(conversionDir);
        return ResolveModelPaths(arcExtractDir);
    }

    //-------------------------------------------------------------------------------
    // モデルファイルから地上マップの俯瞰画像を生成する処理
    //-------------------------------------------------------------------------------
    private static void TryRenderPreviewImage(string? objPath, string? mtlPath, string? daePath, string imagePath)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(objPath) &&
                !string.IsNullOrWhiteSpace(mtlPath) &&
                File.Exists(objPath) &&
                File.Exists(mtlPath))
            {
                ObjTopDownRenderer.RenderAuto(objPath, mtlPath, imagePath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(daePath) && File.Exists(daePath))
            {
                DaeTopDownRenderer.RenderAuto(daePath, imagePath);
            }
        }
        catch
        {
        }
    }

    //-------------------------------------------------------------------------------
    // プレビュー画像の再生成が必要かを判定する処理
    //-------------------------------------------------------------------------------
    private static bool ShouldRenderPreviewImage(string imagePath, params string?[] sourcePaths)
    {
        if (!File.Exists(imagePath))
        {
            return true;
        }

        DateTime imageTime = File.GetLastWriteTimeUtc(imagePath);
        foreach (string? sourcePath in sourcePaths)
        {
            if (!string.IsNullOrWhiteSpace(sourcePath) &&
                File.Exists(sourcePath) &&
                File.GetLastWriteTimeUtc(sourcePath) > imageTime)
            {
                return true;
            }
        }

        return false;
    }

    //-------------------------------------------------------------------------------
    // アーカイブを指定フォルダへ展開する処理
    //-------------------------------------------------------------------------------
    private void EnsureArchiveExtracted(string archivePath, string outputDirectory)
    {
        if (!File.Exists(archivePath))
        {
            return;
        }

        if (Directory.Exists(outputDirectory) &&
            Directory.EnumerateFileSystemEntries(outputDirectory, "*", SearchOption.AllDirectories).Any())
        {
            return;
        }

        Directory.CreateDirectory(outputDirectory);
        TryRunToolkit($"--extract \"{archivePath}\" \"{outputDirectory}\"", Path.GetDirectoryName(archivePath) ?? _fieldMapRoot);
        DeleteToolkitSidecarDirectory(outputDirectory);
    }

    //-------------------------------------------------------------------------------
    // Hocotate Toolkit を実行する処理
    //-------------------------------------------------------------------------------
    private bool TryRunToolkit(string arguments, string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(_toolkitPath) || !File.Exists(_toolkitPath))
        {
            return false;
        }

        try
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _toolkitPath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
        finally
        {
            DeleteToolkitSidecarDirectory(workingDirectory);
        }
    }

    //-------------------------------------------------------------------------------
    // 候補ファイルから地上マップ表示に使いやすいものを選ぶ処理
    //-------------------------------------------------------------------------------
    private static string? PickPreferredFile(IEnumerable<string> paths)
    {
        return paths
            .OrderByDescending(path => string.Equals(Path.GetFileNameWithoutExtension(path), "model", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(path => Path.GetFileNameWithoutExtension(path).Contains("map", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(path => Path.GetFileNameWithoutExtension(path).Contains("course", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(path => Path.GetFileNameWithoutExtension(path).Contains("view", StringComparison.OrdinalIgnoreCase))
            .ThenBy(Path.GetFileName)
            .FirstOrDefault();
    }

    //-------------------------------------------------------------------------------
    // 最初に存在するファイルパスを返す処理
    //-------------------------------------------------------------------------------
    private static string? FindFirstExistingPathOrNull(params string[] candidates)
    {
        return candidates.FirstOrDefault(File.Exists);
    }

    //-------------------------------------------------------------------------------
    // Hocotate Toolkit の副生成フォルダを削除する処理
    //-------------------------------------------------------------------------------
    private static void DeleteToolkitSidecarDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        string sidecarDirectory = Path.Combine(directory, "AI_content");
        if (!Directory.Exists(sidecarDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(sidecarDirectory, true);
        }
        catch
        {
        }
    }

    private sealed record FieldModelPaths(string? ObjPath, string? MtlPath, string? DaePath, string? BmdPath);
}
