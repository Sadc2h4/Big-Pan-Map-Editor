using System.Diagnostics;
using System.Globalization;
using System.Drawing.Imaging;
using System.Xml.Linq;

namespace PikminUnitEditor;

internal sealed class UnitAssetCacheService
{
    private readonly string _arcRoot;
    private readonly string _unitCacheDir;
    private readonly string _imageCacheDir;
    private readonly string _routeCacheDir;
    private readonly IReadOnlyDictionary<string, string> _fallbackImageCatalog;
    private readonly string? _toolkitPath;
    private readonly Func<string, Stream?>? _prettyImageProvider;

    public UnitAssetCacheService(string arcRoot, IReadOnlyDictionary<string, string> fallbackImageCatalog, string? toolkitPath, string? cacheRootOverride = null, Func<string, Stream?>? prettyImageProvider = null)
    {
        _arcRoot = arcRoot;
        string cacheRoot = !string.IsNullOrWhiteSpace(cacheRootOverride)
            ? cacheRootOverride
            : Path.GetDirectoryName(arcRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? arcRoot;
        _unitCacheDir = Path.Combine(cacheRoot, "ユニットキャッシュ");
        _imageCacheDir = Path.Combine(cacheRoot, "画像キャッシュ");
        _routeCacheDir = Path.Combine(cacheRoot, "ルートキャッシュ");
        _fallbackImageCatalog = fallbackImageCatalog;
        _toolkitPath = File.Exists(toolkitPath) ? toolkitPath : null;
        _prettyImageProvider = prettyImageProvider;
    }

    //-------------------------------------------------------------------------------
    // 指定ユニットの展開済みキャッシュを用意する処理
    //-------------------------------------------------------------------------------
    public UnitCacheEntry EnsureUnitCache(string unitName, Action<string, int, int>? progressCallback = null, bool replacePreviewWithPrettyImage = false)
    {
        progressCallback?.Invoke(unitName, 0, 4);
        Directory.CreateDirectory(_unitCacheDir);
        Directory.CreateDirectory(_imageCacheDir);

        string unitDir = Path.Combine(_arcRoot, unitName);
        string cacheDir = Path.Combine(_unitCacheDir, unitName);
        string arcExtractDir = Path.Combine(cacheDir, "arc");
        string textsExtractDir = Path.Combine(cacheDir, "texts");
        string imagePath = Path.Combine(_imageCacheDir, unitName + ".png");

        Directory.CreateDirectory(cacheDir);
        progressCallback?.Invoke(unitName, 1, 4);
        EnsureArchiveExtracted(Path.Combine(unitDir, "arc.szs"), arcExtractDir);
        progressCallback?.Invoke(unitName, 2, 4);
        EnsureArchiveExtracted(Path.Combine(unitDir, "texts.szs"), textsExtractDir);

        string? objPath = FindFirstExistingPathOrNull(
            Path.Combine(arcExtractDir, "view", "view.obj"),
            Path.Combine(arcExtractDir, "view.obj"),
            Path.Combine(unitDir, "arc", "view", "view.obj"),
            Path.Combine(unitDir, "arc", "view.obj"),
            Path.Combine(unitDir, "tmp", "view.obj"));
        string? mtlPath = FindFirstExistingPathOrNull(
            Path.Combine(arcExtractDir, "view", "view.obj.mtl"),
            Path.Combine(arcExtractDir, "view.obj.mtl"),
            Path.Combine(unitDir, "arc", "view", "view.obj.mtl"),
            Path.Combine(unitDir, "arc", "view.obj.mtl"),
            Path.Combine(unitDir, "tmp", "view.obj.mtl"));
        string? daePath = FindFirstExistingPathOrNull(
            Path.Combine(arcExtractDir, "view", "view.dae"),
            Path.Combine(arcExtractDir, "view.dae"));
        string? bmdPath = FindFirstExistingPathOrNull(
            Path.Combine(arcExtractDir, "view", "view.bmd"),
            Path.Combine(arcExtractDir, "view.bmd"),
            Path.Combine(unitDir, "arc", "view.bmd"),
            Path.Combine(unitDir, "tmp", "view.bmd"));

        if ((objPath is null || mtlPath is null) && bmdPath is not null)
        {
            string conversionDir = GetConversionOutputDirectory(bmdPath);
            Directory.CreateDirectory(conversionDir);
            string requestedObjPath = Path.Combine(conversionDir, "view.obj");
            string requestedDaePath = Path.Combine(conversionDir, "view.dae");

            if (objPath is null || mtlPath is null)
            {
                TryRunToolkit($"--bmd2obj \"{bmdPath}\" \"{requestedObjPath}\"", conversionDir);
            }

            if (daePath is null)
            {
                TryRunToolkit($"--bmd2dae \"{bmdPath}\" \"{requestedDaePath}\"", conversionDir);
            }
            DeleteToolkitSidecarDirectory(conversionDir);

            objPath = FindFirstExistingPathOrNull(
                requestedObjPath,
                Path.Combine(conversionDir, "view.obj"),
                Path.Combine(arcExtractDir, "view", "view.obj"),
                Path.Combine(arcExtractDir, "view.obj"));
            mtlPath = FindFirstExistingPathOrNull(
                Path.Combine(conversionDir, "view.obj.mtl"),
                Path.Combine(arcExtractDir, "view", "view.obj.mtl"),
                Path.Combine(arcExtractDir, "view.obj.mtl"));
            daePath = FindFirstExistingPathOrNull(
                requestedDaePath,
                Path.Combine(conversionDir, "view.dae"),
                Path.Combine(arcExtractDir, "view", "view.dae"),
                Path.Combine(arcExtractDir, "view.dae"));
        }

        progressCallback?.Invoke(unitName, 3, 4);
        bool hasPrettyImage = HasPrettyImage(unitName);
        bool copiedPrettyImage = false;
        if (hasPrettyImage && (!File.Exists(imagePath) || replacePreviewWithPrettyImage))
        {
            copiedPrettyImage = TryCopyPrettyImage(unitName, imagePath);
        }

        if ((!hasPrettyImage || (!File.Exists(imagePath) && !copiedPrettyImage)) &&
            ShouldRenderPreviewImage(imagePath, objPath, daePath))
        {
            if (objPath is not null && mtlPath is not null && File.Exists(objPath) && File.Exists(mtlPath))
            {
                ObjTopDownRenderer.RenderAuto(objPath, mtlPath, imagePath);
            }
            else if (daePath is not null && File.Exists(daePath))
            {
                DaeTopDownRenderer.RenderAuto(daePath, imagePath);
            }
        }

        string? layoutPath = FindFirstExistingPathOrNull(
            Path.Combine(textsExtractDir, "layout.txt"),
            Path.Combine(unitDir, "texts", "layout.txt"),
            Path.Combine(unitDir, "tmp", "layout.txt"),
            Path.Combine(unitDir, "大本", "texts", "layout.txt"));
        string? routePath = FindFirstExistingPathOrNull(
            Path.Combine(textsExtractDir, "route.txt"),
            Path.Combine(unitDir, "texts", "route.txt"),
            Path.Combine(unitDir, "tmp", "route.txt"),
            Path.Combine(unitDir, "大本", "texts", "route.txt"));
        string? waterboxPath = FindFirstExistingPathOrNull(
            Path.Combine(textsExtractDir, "waterbox.txt"),
            Path.Combine(unitDir, "texts", "waterbox.txt"),
            Path.Combine(unitDir, "tmp", "waterbox.txt"),
            Path.Combine(unitDir, "大本", "texts", "waterbox.txt"));

        DeleteCacheSidecarDirectories(cacheDir);
        progressCallback?.Invoke(unitName, 4, 4);

        return new UnitCacheEntry(
            unitName,
            cacheDir,
            objPath,
            mtlPath,
            File.Exists(imagePath) ? imagePath : null,
            layoutPath,
            routePath,
            waterboxPath);
    }

    public CacheRefreshResult EnsureAssets(IEnumerable<UnitDefinition> units)
    {
        return EnsureAssets(units, null);
    }

    public CacheRefreshResult EnsureAssets(IEnumerable<UnitDefinition> units, Action<CacheProgressInfo>? progressCallback)
    {
        Directory.CreateDirectory(_imageCacheDir);
        Directory.CreateDirectory(_routeCacheDir);

        UnitDefinition[] distinctUnits = units
            .GroupBy(u => u.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();
        int createdImages = 0;
        int createdRoutes = 0;
        int processedUnits = 0;

        foreach (UnitDefinition unit in distinctUnits)
        {
            progressCallback?.Invoke(new CacheProgressInfo(unit.Name, processedUnits, distinctUnits.Length, "処理中"));
            try
            {
                if (EnsureImageCache(unit))
                {
                    createdImages++;
                }
            }
            catch
            {
                // Skip malformed assets for now so one bad unit does not abort the whole refresh.
            }

            try
            {
                if (EnsureRouteCache(unit.Name))
                {
                    createdRoutes++;
                }
            }
            catch
            {
                // Route extraction failures should not abort image generation for other units.
            }

            processedUnits++;
            progressCallback?.Invoke(new CacheProgressInfo(unit.Name, processedUnits, distinctUnits.Length, "完了"));
        }

        return new CacheRefreshResult(createdImages, createdRoutes);
    }

    //-------------------------------------------------------------------------------
    // キャッシュ展開済み texts フォルダを texts.szs として元ユニットへ再圧縮する処理
    //-------------------------------------------------------------------------------
    public string RepackTextsArchive(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            throw new ArgumentException("unitName is required.", nameof(unitName));
        }

        string cacheDir = Path.Combine(_unitCacheDir, unitName);
        string textsExtractDir = Path.Combine(cacheDir, "texts");
        if (!Directory.Exists(textsExtractDir))
        {
            throw new DirectoryNotFoundException($"texts キャッシュが見つかりません: {textsExtractDir}");
        }

        string unitDir = Path.Combine(_arcRoot, unitName);
        Directory.CreateDirectory(unitDir);
        string outputArchivePath = Path.Combine(unitDir, "texts.szs");

        if (!TryRunToolkit($"--szs \"{textsExtractDir}\" \"{outputArchivePath}\"", Path.GetDirectoryName(outputArchivePath) ?? _arcRoot))
        {
            throw new InvalidOperationException("texts.szs の再圧縮に失敗しました．Hocotate_Toolkit の設定と出力先を確認してください．");
        }

        return outputArchivePath;
    }

    //-------------------------------------------------------------------------------
    // キャッシュ展開済み arc/texts フォルダを元ユニットの szs へ再圧縮する処理
    //-------------------------------------------------------------------------------
    public UnitArchiveRepackResult RepackUnitArchives(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            throw new ArgumentException("unitName is required.", nameof(unitName));
        }

        string cacheDir = Path.Combine(_unitCacheDir, unitName);
        string arcExtractDir = Path.Combine(cacheDir, "arc");
        string textsExtractDir = Path.Combine(cacheDir, "texts");
        string unitDir = Path.Combine(_arcRoot, unitName);
        Directory.CreateDirectory(unitDir);

        string arcArchivePath = RepackArchiveDirectory(arcExtractDir, Path.Combine(unitDir, "arc.szs"), "arc.szs");
        string textsArchivePath = RepackArchiveDirectory(textsExtractDir, Path.Combine(unitDir, "texts.szs"), "texts.szs");
        return new UnitArchiveRepackResult(arcArchivePath, textsArchivePath);
    }

    //-------------------------------------------------------------------------------
    // 指定フォルダを指定 szs ファイルへ再圧縮する処理
    //-------------------------------------------------------------------------------
    private string RepackArchiveDirectory(string sourceDirectory, string outputArchivePath, string archiveLabel)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"{archiveLabel} 用キャッシュが見つかりません: {sourceDirectory}");
        }

        if (!TryRunToolkit($"--szs \"{sourceDirectory}\" \"{outputArchivePath}\"", Path.GetDirectoryName(outputArchivePath) ?? _arcRoot))
        {
            throw new InvalidOperationException($"{archiveLabel} の再圧縮に失敗しました．Hocotate_Toolkit の設定と出力先を確認してください．");
        }

        return outputArchivePath;
    }

    //-------------------------------------------------------------------------------
    // 埋め込みリソースから高品質な真上俯瞰画像を出力パスへ書き出す処理
    //-------------------------------------------------------------------------------
    private bool TryCopyPrettyImage(string unitName, string outputPath)
    {
        if (_prettyImageProvider is null)
        {
            return false;
        }

        try
        {
            using Stream? stream = _prettyImageProvider(unitName);
            if (stream is null)
            {
                return false;
            }

            string? dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string tempPath = Path.Combine(
                string.IsNullOrWhiteSpace(dir) ? AppContext.BaseDirectory : dir,
                $"{Path.GetFileNameWithoutExtension(outputPath)}.{Guid.NewGuid():N}.tmp.png");

            try
            {
                using Bitmap sourceBitmap = new(stream);
                using Bitmap outputBitmap = TrimTransparentMargin(sourceBitmap);
                outputBitmap.Save(tempPath, ImageFormat.Png);

                BitmapFileWriter.ReplaceFileWithRetry(tempPath, outputPath);
            }
            finally
            {
                TryDeleteFile(tempPath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------------
    // pretty 画像の透明余白を除去した Bitmap を生成する処理
    //-------------------------------------------------------------------------------
    private static Bitmap TrimTransparentMargin(Bitmap source)
    {
        Rectangle contentBounds = GetOpaquePixelBounds(source);
        if (contentBounds.Width <= 0 || contentBounds.Height <= 0)
        {
            return new Bitmap(source);
        }

        if (contentBounds.X == 0 &&
            contentBounds.Y == 0 &&
            contentBounds.Width == source.Width &&
            contentBounds.Height == source.Height)
        {
            return new Bitmap(source);
        }

        Bitmap trimmed = new(contentBounds.Width, contentBounds.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(trimmed);
        graphics.Clear(Color.Transparent);
        graphics.DrawImage(
            source,
            new Rectangle(0, 0, trimmed.Width, trimmed.Height),
            contentBounds,
            GraphicsUnit.Pixel);
        return trimmed;
    }

    //-------------------------------------------------------------------------------
    // 画像内の不透明ピクセルが存在する範囲を取得する処理
    //-------------------------------------------------------------------------------
    private static Rectangle GetOpaquePixelBounds(Bitmap source)
    {
        int minX = source.Width;
        int minY = source.Height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (source.GetPixel(x, y).A <= 8)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        return maxX < minX || maxY < minY
            ? Rectangle.Empty
            : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }

    //-------------------------------------------------------------------------------
    // 残った一時ファイルを削除する処理
    //-------------------------------------------------------------------------------
    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

    //-------------------------------------------------------------------------------
    // pretty 画像が埋め込みリソースに登録されているかを確認する処理
    //-------------------------------------------------------------------------------
    private bool HasPrettyImage(string unitName)
    {
        if (_prettyImageProvider is null)
        {
            return false;
        }

        try
        {
            using Stream? stream = _prettyImageProvider(unitName);
            return stream is not null;
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyDictionary<string, string> BuildImageCatalog()
    {
        Dictionary<string, string> result = new(_fallbackImageCatalog, StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists(_imageCacheDir))
        {
            foreach (string file in Directory.GetFiles(_imageCacheDir, "*.png"))
            {
                result[Path.GetFileNameWithoutExtension(file)] = file;
            }
        }

        return result;
    }

    private bool EnsureImageCache(UnitDefinition unit)
    {
        string outputPath = Path.Combine(_imageCacheDir, unit.Name + ".png");
        if (File.Exists(outputPath))
        {
            return false;
        }

        if (TryCopyPrettyImage(unit.Name, outputPath))
        {
            return true;
        }

        string unitDir = Path.Combine(_arcRoot, unit.Name);
        string arcDir = Path.Combine(unitDir, "arc");
        EnsureArchiveExtracted(Path.Combine(unitDir, "arc.szs"), arcDir);

        string? objPath = FindFirstExistingPathOrNull(
            Path.Combine(arcDir, "view", "view.obj"),
            Path.Combine(arcDir, "view.obj"),
            Path.Combine(unitDir, "model", "view.obj"),
            Path.Combine(arcDir, "model", "view.obj"));
        string? mtlPath = FindFirstExistingPathOrNull(
            Path.Combine(arcDir, "view", "view.obj.mtl"),
            Path.Combine(arcDir, "view.obj.mtl"),
            Path.Combine(unitDir, "model", "view.obj.mtl"),
            Path.Combine(arcDir, "model", "view.obj.mtl"));
        string? daePath = FindFirstExistingPathOrNull(
            Path.Combine(arcDir, "view", "view.dae"),
            Path.Combine(arcDir, "view.dae"),
            Path.Combine(unitDir, "model", "view.dae"),
            Path.Combine(arcDir, "model", "view.dae"));
        string? bmdPath = FindFirstExistingPathOrNull(
            Path.Combine(arcDir, "view", "view.bmd"),
            Path.Combine(arcDir, "view.bmd"),
            Path.Combine(unitDir, "model", "view.bmd"),
            Path.Combine(arcDir, "model", "view.bmd"));

        if ((objPath is null || mtlPath is null) && daePath is null && bmdPath is not null)
        {
            string conversionDir = GetConversionOutputDirectory(bmdPath);
            Directory.CreateDirectory(conversionDir);
            string requestedObjPath = Path.Combine(conversionDir, "view.obj");
            string requestedDaePath = Path.Combine(conversionDir, "view.dae");

            if (objPath is null || mtlPath is null)
            {
                TryRunToolkit($"--bmd2obj \"{bmdPath}\" \"{requestedObjPath}\"", conversionDir);
            }

            if (daePath is null && objPath is null)
            {
                TryRunToolkit($"--bmd2dae \"{bmdPath}\" \"{requestedDaePath}\"", conversionDir);
            }
            DeleteToolkitSidecarDirectory(conversionDir);

            objPath = FindFirstExistingPathOrNull(
                requestedObjPath,
                Path.Combine(Path.GetDirectoryName(bmdPath) ?? arcDir, "view.obj"),
                Path.Combine(arcDir, "view", "view.obj"),
                Path.Combine(arcDir, "view.obj"),
                Path.Combine(unitDir, "model", "view.obj"),
                Path.Combine(arcDir, "model", "view.obj"));
            mtlPath = FindFirstExistingPathOrNull(
                Path.Combine(Path.GetDirectoryName(objPath ?? requestedObjPath) ?? conversionDir, "view.obj.mtl"),
                Path.Combine(arcDir, "view", "view.obj.mtl"),
                Path.Combine(arcDir, "view.obj.mtl"),
                Path.Combine(unitDir, "model", "view.obj.mtl"),
                Path.Combine(arcDir, "model", "view.obj.mtl"));
            daePath = FindFirstExistingPathOrNull(
                requestedDaePath,
                Path.Combine(Path.GetDirectoryName(bmdPath) ?? arcDir, "view.dae"),
                Path.Combine(arcDir, "view", "view.dae"),
                Path.Combine(arcDir, "view.dae"),
                Path.Combine(unitDir, "model", "view.dae"),
                Path.Combine(arcDir, "model", "view.dae"));
        }

        if (objPath is not null && mtlPath is not null && File.Exists(objPath) && File.Exists(mtlPath))
        {
            ObjTopDownRenderer.Render(objPath, mtlPath, outputPath, unit.Width, unit.Height);
            DeleteCacheSidecarDirectories(unitDir);
            return File.Exists(outputPath);
        }

        if (daePath is not null && File.Exists(daePath))
        {
            DaeTopDownRenderer.Render(daePath, outputPath, unit.Width, unit.Height);
            DeleteCacheSidecarDirectories(unitDir);
            return File.Exists(outputPath);
        }

        DeleteCacheSidecarDirectories(unitDir);
        return false;
    }

    private bool EnsureRouteCache(string unitName)
    {
        string outputPath = Path.Combine(_routeCacheDir, unitName + ".txt");
        if (File.Exists(outputPath))
        {
            return false;
        }

        string unitDir = Path.Combine(_arcRoot, unitName);
        string textsDir = Path.Combine(unitDir, "texts");
        EnsureArchiveExtracted(Path.Combine(unitDir, "texts.szs"), textsDir);

        string[] candidates =
        {
            Path.Combine(textsDir, "route.txt"),
            Path.Combine(unitDir, "tmp", "route.txt"),
            Path.Combine(unitDir, "大本", "texts", "route.txt")
        };

        string? source = candidates.FirstOrDefault(File.Exists);
        if (source is null)
        {
            return false;
        }

        File.Copy(source, outputPath, false);
        return true;
    }

    private void EnsureArchiveExtracted(string archivePath, string outputDirectory)
    {
        if (!File.Exists(archivePath) || Directory.Exists(outputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(outputDirectory);
        if (!TryRunToolkit($"--extract \"{archivePath}\" \"{outputDirectory}\"", Path.GetDirectoryName(archivePath) ?? _arcRoot))
        {
            return;
        }

        DeleteToolkitSidecarDirectory(outputDirectory);
    }

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
    // Hocotate Toolkit が作成する用途外の副生成フォルダを削除する処理
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

    //-------------------------------------------------------------------------------
    // キャッシュ周辺に作成された用途外の副生成フォルダを削除する処理
    //-------------------------------------------------------------------------------
    private void DeleteCacheSidecarDirectories(string cacheDirectory)
    {
        DeleteToolkitSidecarDirectory(cacheDirectory);
        DeleteToolkitSidecarDirectory(_unitCacheDir);
        DeleteToolkitSidecarDirectory(Path.GetDirectoryName(_unitCacheDir) ?? string.Empty);
    }

    private static string FindFirstExistingPath(params string[] candidates)
    {
        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static string? FindFirstExistingPathOrNull(params string[] candidates)
    {
        return candidates.FirstOrDefault(File.Exists);
    }

    private static string GetConversionOutputDirectory(string bmdPath)
    {
        string bmdDirectory = Path.GetDirectoryName(bmdPath) ?? string.Empty;
        if (string.Equals(Path.GetFileName(bmdDirectory), "view", StringComparison.OrdinalIgnoreCase))
        {
            return bmdDirectory;
        }

        return string.Equals(Path.GetFileName(bmdDirectory), "model", StringComparison.OrdinalIgnoreCase)
            ? bmdDirectory
            : Path.Combine(bmdDirectory, "view");
    }

    private static bool ShouldRenderPreviewImage(string imagePath, string? objPath, string? daePath)
    {
        if (!File.Exists(imagePath))
        {
            return true;
        }

        float? expectedAspect = null;
        if (!string.IsNullOrWhiteSpace(objPath) && File.Exists(objPath))
        {
            expectedAspect = GetObjAspect(objPath);
        }
        else if (!string.IsNullOrWhiteSpace(daePath) && File.Exists(daePath))
        {
            DaeScene scene = DaeScene.Load(daePath);
            expectedAspect = GetAspect(scene.Vertices);
        }

        if (expectedAspect is null || expectedAspect <= 0.0001f)
        {
            return false;
        }

        using Image image = Image.FromFile(imagePath);
        float imageAspect = image.Width / (float)Math.Max(image.Height, 1);
        return Math.Abs(imageAspect - expectedAspect.Value) > 0.08f;
    }

    private static float GetAspect(IReadOnlyList<ObjVertex> vertices)
    {
        if (vertices.Count == 0)
        {
            return 1f;
        }

        float spanX = Math.Max(vertices.Max(v => v.X) - vertices.Min(v => v.X), 1f);
        float spanZ = Math.Max(vertices.Max(v => v.Z) - vertices.Min(v => v.Z), 1f);
        return spanX / spanZ;
    }

    private static float GetObjAspect(string objPath)
    {
        List<ObjVertex> vertices = new();
        foreach (string line in File.ReadLines(objPath))
        {
            if (!line.StartsWith("v ", StringComparison.Ordinal))
            {
                continue;
            }

            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                continue;
            }

            if (float.TryParse(parts[1], CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[3], CultureInfo.InvariantCulture, out float z))
            {
                vertices.Add(new ObjVertex(x, 0f, z));
            }
        }

        return GetAspect(vertices);
    }

}

internal sealed record CacheRefreshResult(int CreatedImages, int CreatedRoutes);
internal sealed record CacheProgressInfo(string UnitName, int CompletedUnits, int TotalUnits, string Stage);
internal sealed record UnitCacheEntry(
    string UnitName,
    string CacheDirectory,
    string? ObjPath,
    string? MtlPath,
    string? PreviewImagePath,
    string? LayoutPath,
    string? RoutePath,
    string? WaterboxPath);

internal sealed record UnitArchiveRepackResult(string ArcArchivePath, string TextsArchivePath);

internal static class BitmapFileWriter
{
    //-------------------------------------------------------------------------------
    // Bitmap を一時ファイル経由で PNG 保存し，既存ファイルを安全に置換する処理
    //-------------------------------------------------------------------------------
    public static void SaveBitmapAsPng(Bitmap bitmap, string outputPath)
    {
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = Path.Combine(
            string.IsNullOrWhiteSpace(directory) ? AppContext.BaseDirectory : directory,
            $"{Path.GetFileNameWithoutExtension(outputPath)}.{Guid.NewGuid():N}.tmp.png");

        try
        {
            bitmap.Save(tempPath, ImageFormat.Png);
            ReplaceFileWithRetry(tempPath, outputPath);
        }
        finally
        {
            TryDeleteFile(tempPath);
        }
    }

    //-------------------------------------------------------------------------------
    // 一時ファイルを出力先へ置換する処理
    //-------------------------------------------------------------------------------
    public static void ReplaceFileWithRetry(string tempPath, string outputPath)
    {
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                File.Move(tempPath, outputPath, true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(80 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(80 * attempt);
            }
        }

        File.Move(tempPath, outputPath, true);
    }

    //-------------------------------------------------------------------------------
    // 残った一時ファイルを削除する処理
    //-------------------------------------------------------------------------------
    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}

internal static class ObjTopDownRenderer
{
    private const int PixelsPerCell = 128;
    private const int AutoMaxDimension = 2048;
    private const int AutoMinDimension = 256;

    public static void Render(string objPath, string mtlPath, string outputPath, int unitWidth, int unitHeight)
    {
        ObjScene scene = ObjScene.Load(objPath, mtlPath);
        using Bitmap bitmap = new(Math.Max(unitWidth * PixelsPerCell, 64), Math.Max(unitHeight * PixelsPerCell, 64));
        RenderScene(scene, bitmap, outputPath);
    }

    public static void RenderAuto(string objPath, string mtlPath, string outputPath)
    {
        ObjScene scene = ObjScene.Load(objPath, mtlPath);
        using Bitmap bitmap = CreateAutoBitmap(scene.Vertices);
        RenderScene(scene, bitmap, outputPath);
    }

    private static void RenderScene(ObjScene scene, Bitmap bitmap, string outputPath)
    {
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        if (scene.Vertices.Count == 0 || scene.Faces.Count == 0)
        {
            BitmapFileWriter.SaveBitmapAsPng(bitmap, outputPath);
            return;
        }

        float minX = scene.Vertices.Min(v => v.X);
        float maxX = scene.Vertices.Max(v => v.X);
        float minZ = scene.Vertices.Min(v => v.Z);
        float maxZ = scene.Vertices.Max(v => v.Z);
        float spanX = Math.Max(maxX - minX, 1f);
        float spanZ = Math.Max(maxZ - minZ, 1f);

        float scaleX = bitmap.Width / spanX;
        float scaleZ = bitmap.Height / spanZ;
        float scale = Math.Min(scaleX, scaleZ);
        float offsetX = (bitmap.Width - spanX * scale) * 0.5f;
        float offsetY = (bitmap.Height - spanZ * scale) * 0.5f;

        float[,] depth = new float[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                depth[x, y] = float.NegativeInfinity;
            }
        }

        foreach (ObjFace face in scene.Faces)
        {
            if (face.Indices.Count < 3)
            {
                continue;
            }

            for (int i = 1; i < face.Indices.Count - 1; i++)
            {
                ObjIndex ia = face.Indices[0];
                ObjIndex ib = face.Indices[i];
                ObjIndex ic = face.Indices[i + 1];

                if (!HasValidVertexIndices(scene.Vertices, ia, ib, ic))
                {
                    continue;
                }

                ObjVertex va = scene.Vertices[ia.Vertex];
                ObjVertex vb = scene.Vertices[ib.Vertex];
                ObjVertex vc = scene.Vertices[ic.Vertex];

                if (!IsTopVisible(va, vb, vc))
                {
                    continue;
                }

                PointF pa = Project(va, minX, minZ, scale, offsetX, offsetY, bitmap.Height);
                PointF pb = Project(vb, minX, minZ, scale, offsetX, offsetY, bitmap.Height);
                PointF pc = Project(vc, minX, minZ, scale, offsetX, offsetY, bitmap.Height);

                Rectangle bounds = GetBounds(pa, pb, pc, bitmap.Width, bitmap.Height);
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    continue;
                }

                Bitmap? texture = scene.GetTexture(face.MaterialName);
                for (int py = bounds.Top; py < bounds.Bottom; py++)
                {
                    for (int px = bounds.Left; px < bounds.Right; px++)
                    {
                        PointF p = new(px + 0.5f, py + 0.5f);
                        if (!TryGetBarycentric(p, pa, pb, pc, out float w0, out float w1, out float w2))
                        {
                            continue;
                        }

                        float yDepth = va.Y * w0 + vb.Y * w1 + vc.Y * w2;
                        if (yDepth < depth[px, py])
                        {
                            continue;
                        }

                        Color color = SampleColor(scene, texture, ia, ib, ic, w0, w1, w2);
                        bitmap.SetPixel(px, py, color);
                        depth[px, py] = yDepth;
                    }
                }
            }
        }

        BitmapFileWriter.SaveBitmapAsPng(bitmap, outputPath);
    }

    private static bool IsTopVisible(ObjVertex a, ObjVertex b, ObjVertex c)
    {
        float ux = b.X - a.X;
        float uy = b.Y - a.Y;
        float uz = b.Z - a.Z;
        float vx = c.X - a.X;
        float vy = c.Y - a.Y;
        float vz = c.Z - a.Z;
        float ny = uz * vx - ux * vz;
        float nx = uy * vz - uz * vy;
        float nz = ux * vy - uy * vx;
        float length = MathF.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
        if (length < 0.0001f)
        {
            return false;
        }

        return (ny / length) >= 0.45f;
    }

    private static PointF Project(ObjVertex vertex, float minX, float minZ, float scale, float offsetX, float offsetY, int bitmapHeight)
    {
        float x = offsetX + (vertex.X - minX) * scale;
        float y = offsetY + (vertex.Z - minZ) * scale;
        return new PointF(x, bitmapHeight - y);
    }

    private static Rectangle GetBounds(PointF a, PointF b, PointF c, int width, int height)
    {
        int minX = Math.Clamp((int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))), 0, width - 1);
        int minY = Math.Clamp((int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))), 0, height - 1);
        int maxX = Math.Clamp((int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))), 0, width);
        int maxY = Math.Clamp((int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))), 0, height);
        return Rectangle.FromLTRB(minX, minY, maxX, maxY);
    }

    private static bool TryGetBarycentric(PointF p, PointF a, PointF b, PointF c, out float w0, out float w1, out float w2)
    {
        float denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
        if (Math.Abs(denom) < 0.0001f)
        {
            w0 = w1 = w2 = 0f;
            return false;
        }

        w0 = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
        w1 = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
        w2 = 1f - w0 - w1;
        return w0 >= 0f && w1 >= 0f && w2 >= 0f;
    }

    private static Color SampleColor(ObjScene scene, Bitmap? texture, ObjIndex ia, ObjIndex ib, ObjIndex ic, float w0, float w1, float w2)
    {
        if (texture is null || ia.TexCoord < 0 || ib.TexCoord < 0 || ic.TexCoord < 0)
        {
            return scene.GetMaterialColorFallback();
        }

        if (ia.TexCoord >= scene.TexCoords.Count || ib.TexCoord >= scene.TexCoords.Count || ic.TexCoord >= scene.TexCoords.Count)
        {
            return scene.GetMaterialColorFallback();
        }

        ObjTexCoord ta = scene.TexCoords[ia.TexCoord];
        ObjTexCoord tb = scene.TexCoords[ib.TexCoord];
        ObjTexCoord tc = scene.TexCoords[ic.TexCoord];
        float u = ta.U * w0 + tb.U * w1 + tc.U * w2;
        float v = ta.V * w0 + tb.V * w1 + tc.V * w2;
        int x = ToWrappedTexturePixel(u, texture.Width);
        int y = ToWrappedTexturePixel(1f - v, texture.Height);
        return texture.GetPixel(x, y);
    }

    //-------------------------------------------------------------------------------
    // 0-1 外の UV をリピート扱いでテクスチャ座標へ変換する処理
    //-------------------------------------------------------------------------------
    private static int ToWrappedTexturePixel(float value, int size)
    {
        if (size <= 1)
        {
            return 0;
        }

        float wrapped = value - MathF.Floor(value);
        return Math.Clamp((int)(wrapped * (size - 1)), 0, size - 1);
    }

    private static bool HasValidVertexIndices(IReadOnlyList<ObjVertex> vertices, ObjIndex ia, ObjIndex ib, ObjIndex ic)
    {
        return ia.Vertex >= 0 && ia.Vertex < vertices.Count
            && ib.Vertex >= 0 && ib.Vertex < vertices.Count
            && ic.Vertex >= 0 && ic.Vertex < vertices.Count;
    }

    private static Bitmap CreateAutoBitmap(IReadOnlyList<ObjVertex> vertices)
    {
        if (vertices.Count == 0)
        {
            return new Bitmap(AutoMinDimension, AutoMinDimension);
        }

        float spanX = Math.Max(vertices.Max(v => v.X) - vertices.Min(v => v.X), 1f);
        float spanZ = Math.Max(vertices.Max(v => v.Z) - vertices.Min(v => v.Z), 1f);
        float longest = Math.Max(spanX, spanZ);
        float scale = AutoMaxDimension / longest;
        int width = Math.Max((int)Math.Ceiling(spanX * scale), AutoMinDimension);
        int height = Math.Max((int)Math.Ceiling(spanZ * scale), AutoMinDimension);
        return new Bitmap(width, height);
    }
}

internal static class DaeTopDownRenderer
{
    private const int PixelsPerCell = 128;
    private const int AutoMaxDimension = 2048;
    private const int AutoMinDimension = 256;

    public static void Render(string daePath, string outputPath, int unitWidth, int unitHeight)
    {
        DaeScene scene = DaeScene.Load(daePath);
        using Bitmap bitmap = new(Math.Max(unitWidth * PixelsPerCell, 64), Math.Max(unitHeight * PixelsPerCell, 64));
        RenderScene(scene, bitmap, outputPath);
    }

    public static void RenderAuto(string daePath, string outputPath)
    {
        DaeScene scene = DaeScene.Load(daePath);
        using Bitmap bitmap = CreateAutoBitmap(scene.Vertices);
        RenderScene(scene, bitmap, outputPath);
    }

    private static void RenderScene(DaeScene scene, Bitmap bitmap, string outputPath)
    {
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        if (scene.Vertices.Count == 0 || scene.Faces.Count == 0)
        {
            BitmapFileWriter.SaveBitmapAsPng(bitmap, outputPath);
            return;
        }

        float minX = scene.Vertices.Min(v => v.X);
        float maxX = scene.Vertices.Max(v => v.X);
        float minZ = scene.Vertices.Min(v => v.Z);
        float maxZ = scene.Vertices.Max(v => v.Z);
        float spanX = Math.Max(maxX - minX, 1f);
        float spanZ = Math.Max(maxZ - minZ, 1f);
        float scale = Math.Min(bitmap.Width / spanX, bitmap.Height / spanZ);
        float offsetX = (bitmap.Width - spanX * scale) * 0.5f;
        float offsetY = (bitmap.Height - spanZ * scale) * 0.5f;

        float[,] depth = new float[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                depth[x, y] = float.NegativeInfinity;
            }
        }

        foreach (ObjFace face in scene.Faces)
        {
            for (int i = 1; i < face.Indices.Count - 1; i++)
            {
                ObjIndex ia = face.Indices[0];
                ObjIndex ib = face.Indices[i];
                ObjIndex ic = face.Indices[i + 1];

                if (!HasValidVertexIndices(scene.Vertices, ia, ib, ic))
                {
                    continue;
                }

                ObjVertex va = scene.Vertices[ia.Vertex];
                ObjVertex vb = scene.Vertices[ib.Vertex];
                ObjVertex vc = scene.Vertices[ic.Vertex];

                if (!IsTopVisible(va, vb, vc))
                {
                    continue;
                }

                PointF pa = Project(va, minX, minZ, scale, offsetX, offsetY, bitmap.Height);
                PointF pb = Project(vb, minX, minZ, scale, offsetX, offsetY, bitmap.Height);
                PointF pc = Project(vc, minX, minZ, scale, offsetX, offsetY, bitmap.Height);
                Rectangle bounds = GetBounds(pa, pb, pc, bitmap.Width, bitmap.Height);
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    continue;
                }

                for (int py = bounds.Top; py < bounds.Bottom; py++)
                {
                    for (int px = bounds.Left; px < bounds.Right; px++)
                    {
                        PointF p = new(px + 0.5f, py + 0.5f);
                        if (!TryGetBarycentric(p, pa, pb, pc, out float w0, out float w1, out float w2))
                        {
                            continue;
                        }

                        float yDepth = va.Y * w0 + vb.Y * w1 + vc.Y * w2;
                        if (yDepth < depth[px, py])
                        {
                            continue;
                        }

                        Color color = SampleColor(scene.Texture, scene, ia, ib, ic, w0, w1, w2);
                        bitmap.SetPixel(px, py, color);
                        depth[px, py] = yDepth;
                    }
                }
            }
        }

        BitmapFileWriter.SaveBitmapAsPng(bitmap, outputPath);
    }

    private static PointF Project(ObjVertex vertex, float minX, float minZ, float scale, float offsetX, float offsetY, int bitmapHeight)
    {
        float x = offsetX + (vertex.X - minX) * scale;
        float y = offsetY + (vertex.Z - minZ) * scale;
        return new PointF(x, bitmapHeight - y);
    }

    private static bool IsTopVisible(ObjVertex a, ObjVertex b, ObjVertex c)
    {
        float ux = b.X - a.X;
        float uy = b.Y - a.Y;
        float uz = b.Z - a.Z;
        float vx = c.X - a.X;
        float vy = c.Y - a.Y;
        float vz = c.Z - a.Z;
        float ny = uz * vx - ux * vz;
        float nx = uy * vz - uz * vy;
        float nz = ux * vy - uy * vx;
        float length = MathF.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
        if (length < 0.0001f)
        {
            return false;
        }

        return (ny / length) >= 0.45f;
    }

    private static Rectangle GetBounds(PointF a, PointF b, PointF c, int width, int height)
    {
        int minX = Math.Clamp((int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))), 0, width - 1);
        int minY = Math.Clamp((int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))), 0, height - 1);
        int maxX = Math.Clamp((int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))), 0, width);
        int maxY = Math.Clamp((int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))), 0, height);
        return Rectangle.FromLTRB(minX, minY, maxX, maxY);
    }

    private static bool TryGetBarycentric(PointF p, PointF a, PointF b, PointF c, out float w0, out float w1, out float w2)
    {
        float denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
        if (Math.Abs(denom) < 0.0001f)
        {
            w0 = w1 = w2 = 0f;
            return false;
        }

        w0 = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
        w1 = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
        w2 = 1f - w0 - w1;
        return w0 >= 0f && w1 >= 0f && w2 >= 0f;
    }

    private static Color SampleColor(Bitmap? texture, DaeScene scene, ObjIndex ia, ObjIndex ib, ObjIndex ic, float w0, float w1, float w2)
    {
        if (texture is null || ia.TexCoord < 0 || ib.TexCoord < 0 || ic.TexCoord < 0 || scene.TexCoords.Count == 0)
        {
            return Color.FromArgb(200, 180, 150);
        }

        if (ia.TexCoord >= scene.TexCoords.Count || ib.TexCoord >= scene.TexCoords.Count || ic.TexCoord >= scene.TexCoords.Count)
        {
            return Color.FromArgb(200, 180, 150);
        }

        ObjTexCoord ta = scene.TexCoords[ia.TexCoord];
        ObjTexCoord tb = scene.TexCoords[ib.TexCoord];
        ObjTexCoord tc = scene.TexCoords[ic.TexCoord];
        float u = ta.U * w0 + tb.U * w1 + tc.U * w2;
        float v = ta.V * w0 + tb.V * w1 + tc.V * w2;
        int x = ToWrappedTexturePixel(u, texture.Width);
        int y = ToWrappedTexturePixel(1f - v, texture.Height);
        return texture.GetPixel(x, y);
    }

    //-------------------------------------------------------------------------------
    // 0-1 外の UV をリピート扱いでテクスチャ座標へ変換する処理
    //-------------------------------------------------------------------------------
    private static int ToWrappedTexturePixel(float value, int size)
    {
        if (size <= 1)
        {
            return 0;
        }

        float wrapped = value - MathF.Floor(value);
        return Math.Clamp((int)(wrapped * (size - 1)), 0, size - 1);
    }

    private static bool HasValidVertexIndices(IReadOnlyList<ObjVertex> vertices, ObjIndex ia, ObjIndex ib, ObjIndex ic)
    {
        return ia.Vertex >= 0 && ia.Vertex < vertices.Count
            && ib.Vertex >= 0 && ib.Vertex < vertices.Count
            && ic.Vertex >= 0 && ic.Vertex < vertices.Count;
    }

    private static Bitmap CreateAutoBitmap(IReadOnlyList<ObjVertex> vertices)
    {
        if (vertices.Count == 0)
        {
            return new Bitmap(AutoMinDimension, AutoMinDimension);
        }

        float spanX = Math.Max(vertices.Max(v => v.X) - vertices.Min(v => v.X), 1f);
        float spanZ = Math.Max(vertices.Max(v => v.Z) - vertices.Min(v => v.Z), 1f);
        float longest = Math.Max(spanX, spanZ);
        float scale = AutoMaxDimension / longest;
        int width = Math.Max((int)Math.Ceiling(spanX * scale), AutoMinDimension);
        int height = Math.Max((int)Math.Ceiling(spanZ * scale), AutoMinDimension);
        return new Bitmap(width, height);
    }
}

internal sealed class DaeScene
{
    public List<ObjVertex> Vertices { get; } = new();
    public List<ObjTexCoord> TexCoords { get; } = new();
    public List<ObjFace> Faces { get; } = new();
    public Bitmap? Texture { get; private set; }

    public static DaeScene Load(string daePath)
    {
        DaeScene scene = new();
        XDocument doc = XDocument.Load(daePath);
        XNamespace ns = doc.Root!.Name.Namespace;
        string baseDir = Path.GetDirectoryName(daePath) ?? string.Empty;

        string? firstImage = doc.Descendants(ns + "library_images")
            .Descendants(ns + "init_from")
            .Select(x => Path.Combine(baseDir, Path.GetFileName(Uri.UnescapeDataString(x.Value.Trim()))))
            .FirstOrDefault(File.Exists);
        if (firstImage is not null)
        {
            scene.Texture = new Bitmap(firstImage);
        }

        foreach (XElement mesh in doc.Descendants(ns + "mesh"))
        {
            Dictionary<string, float[]> arrays = mesh.Elements(ns + "source")
                .Where(s => s.Attribute("id") is not null)
                .ToDictionary(
                    s => s.Attribute("id")!.Value,
                    s => ParseFloatArray(s.Element(ns + "float_array")?.Value),
                    StringComparer.OrdinalIgnoreCase);

            XElement? positionsSource = mesh.Elements(ns + "source")
                .FirstOrDefault(s => (string?)s.Attribute("id") is string id && id.Contains("positions", StringComparison.OrdinalIgnoreCase));
            if (positionsSource is null)
            {
                continue;
            }

            if (!arrays.TryGetValue(positionsSource.Attribute("id")!.Value, out float[]? positions) || positions.Length == 0)
            {
                continue;
            }

            int vertexBase = scene.Vertices.Count;
            for (int i = 0; i + 2 < positions.Length; i += 3)
            {
                scene.Vertices.Add(new ObjVertex(positions[i], positions[i + 1], positions[i + 2]));
            }

            XElement? texSource = mesh.Elements(ns + "source")
                .FirstOrDefault(s => (string?)s.Attribute("id") is string id && id.Contains("tex0", StringComparison.OrdinalIgnoreCase));
            int texBase = scene.TexCoords.Count;
            if (texSource is not null)
            {
                if (!arrays.TryGetValue(texSource.Attribute("id")!.Value, out float[]? texCoords))
                {
                    texCoords = Array.Empty<float>();
                }

                for (int i = 0; i + 1 < texCoords.Length; i += 2)
                {
                    scene.TexCoords.Add(new ObjTexCoord(texCoords[i], texCoords[i + 1]));
                }
            }

            foreach (XElement polylist in mesh.Elements(ns + "polylist"))
            {
                int[] vcounts = ParseIntArray(polylist.Element(ns + "vcount")?.Value);
                int[] indices = ParseIntArray(polylist.Element(ns + "p")?.Value);
                XElement[] inputs = polylist.Elements(ns + "input").ToArray();
                int stride = inputs.Length == 0
                    ? 1
                    : inputs.Max(i => (int?)i.Attribute("offset") ?? 0) + 1;
                int vertexOffset = inputs
                    .Where(i => string.Equals((string?)i.Attribute("semantic"), "VERTEX", StringComparison.OrdinalIgnoreCase))
                    .Select(i => (int?)i.Attribute("offset") ?? 0)
                    .DefaultIfEmpty(0)
                    .First();
                int? texOffset = inputs
                    .Where(i => string.Equals((string?)i.Attribute("semantic"), "TEXCOORD", StringComparison.OrdinalIgnoreCase))
                    .Select(i => (int?)i.Attribute("offset") ?? 0)
                    .Cast<int?>()
                    .FirstOrDefault();
                int cursor = 0;

                foreach (int count in vcounts)
                {
                    List<ObjIndex> face = new();
                    for (int i = 0; i < count; i++)
                    {
                        int baseIndex = cursor + (i * stride);
                        if (baseIndex + vertexOffset >= indices.Length)
                        {
                            break;
                        }

                        int vertexIndex = indices[baseIndex + vertexOffset];
                        int texIndex = -1;
                        if (texSource is not null)
                        {
                            int sourceTexIndex = texOffset is int actualTexOffset && baseIndex + actualTexOffset < indices.Length
                                ? indices[baseIndex + actualTexOffset]
                                : vertexIndex;
                            int resolvedTexIndex = texBase + sourceTexIndex;
                            if (resolvedTexIndex >= 0 && resolvedTexIndex < scene.TexCoords.Count)
                            {
                                texIndex = resolvedTexIndex;
                            }
                        }

                        int resolvedVertexIndex = vertexBase + vertexIndex;
                        if (resolvedVertexIndex < 0 || resolvedVertexIndex >= scene.Vertices.Count)
                        {
                            continue;
                        }

                        face.Add(new ObjIndex(resolvedVertexIndex, texIndex));
                    }

                    if (face.Count >= 3)
                    {
                        scene.Faces.Add(new ObjFace(null, face));
                    }

                    cursor += count * stride;
                    if (cursor > indices.Length)
                    {
                        break;
                    }
                }
            }
        }

        return scene;
    }

    private static float[] ParseFloatArray(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? Array.Empty<float>()
            : text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();
    }

    private static int[] ParseIntArray(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? Array.Empty<int>()
            : text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => int.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();
    }
}

internal sealed class ObjScene
{
    private readonly Dictionary<string, Bitmap> _textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Color> _materialColors = new(StringComparer.OrdinalIgnoreCase);

    private ObjScene()
    {
    }

    public List<ObjVertex> Vertices { get; } = new();
    public List<ObjTexCoord> TexCoords { get; } = new();
    public List<ObjFace> Faces { get; } = new();

    public static ObjScene Load(string objPath, string? mtlPath)
    {
        ObjScene scene = new();
        Dictionary<string, string> materialTextures = string.IsNullOrWhiteSpace(mtlPath) || !File.Exists(mtlPath)
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : ParseMtl(mtlPath);
        string? currentMaterial = null;

        foreach (string rawLine in File.ReadLines(objPath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("v ", StringComparison.Ordinal))
            {
                string[] parts = SplitParts(line, 4);
                scene.Vertices.Add(new ObjVertex(
                    ParseFloat(parts[1]),
                    ParseFloat(parts[2]),
                    ParseFloat(parts[3])));
            }
            else if (line.StartsWith("vt ", StringComparison.Ordinal))
            {
                string[] parts = SplitParts(line, 3);
                scene.TexCoords.Add(new ObjTexCoord(ParseFloat(parts[1]), ParseFloat(parts[2])));
            }
            else if (line.StartsWith("usemtl ", StringComparison.Ordinal))
            {
                currentMaterial = line["usemtl ".Length..].Trim();
            }
            else if (line.StartsWith("f ", StringComparison.Ordinal))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                List<ObjIndex> indices = new();
                for (int i = 1; i < parts.Length; i++)
                {
                    string[] tokens = parts[i].Split('/');
                    if (!int.TryParse(tokens[0], CultureInfo.InvariantCulture, out int vRaw))
                    {
                        continue;
                    }

                    int v = vRaw > 0 ? vRaw - 1 : scene.Vertices.Count + vRaw;
                    int vt = tokens.Length > 1 && !string.IsNullOrWhiteSpace(tokens[1])
                        && int.TryParse(tokens[1], CultureInfo.InvariantCulture, out int vtRaw)
                        ? (vtRaw > 0 ? vtRaw - 1 : scene.TexCoords.Count + vtRaw)
                        : -1;

                    if (v < 0 || v >= scene.Vertices.Count)
                    {
                        continue;
                    }

                    if (vt < 0 || vt >= scene.TexCoords.Count)
                    {
                        vt = -1;
                    }

                    indices.Add(new ObjIndex(v, vt));
                }

                if (indices.Count >= 3)
                {
                    scene.Faces.Add(new ObjFace(currentMaterial, indices));
                }
            }
        }

        foreach ((string material, string texturePath) in materialTextures)
        {
            if (File.Exists(texturePath))
            {
                scene._textures[material] = new Bitmap(texturePath);
            }
        }

        return scene;
    }

    public Bitmap? GetTexture(string? materialName)
    {
        if (materialName is null)
        {
            return null;
        }

        return _textures.TryGetValue(materialName, out Bitmap? texture) ? texture : null;
    }

    public Color GetMaterialColorFallback()
    {
        return Color.FromArgb(200, 180, 150);
    }

    private static Dictionary<string, string> ParseMtl(string mtlPath)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        string? currentMaterial = null;
        string baseDir = Path.GetDirectoryName(mtlPath) ?? string.Empty;

        foreach (string rawLine in File.ReadLines(mtlPath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("newmtl ", StringComparison.OrdinalIgnoreCase))
            {
                currentMaterial = line["newmtl ".Length..].Trim();
            }
            else if (currentMaterial is not null && line.StartsWith("map_kd ", StringComparison.OrdinalIgnoreCase))
            {
                string path = line["map_kd ".Length..].Trim().Trim('"');
                string resolved = Path.IsPathRooted(path)
                    ? path
                    : Path.GetFullPath(Path.Combine(baseDir, path));
                result[currentMaterial] = resolved;
            }
        }

        return result;
    }

    private static string[] SplitParts(string line, int expectedMin)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < expectedMin)
        {
            throw new FormatException($"OBJ line parse failed: {line}");
        }

        return parts;
    }

    private static float ParseFloat(string text)
    {
        return float.Parse(text, CultureInfo.InvariantCulture);
    }
}

internal sealed record ObjVertex(float X, float Y, float Z);
internal sealed record ObjTexCoord(float U, float V);
internal sealed record ObjIndex(int Vertex, int TexCoord);
internal sealed record ObjFace(string? MaterialName, IReadOnlyList<ObjIndex> Indices);
