using System.Diagnostics;

namespace PikminUnitEditor;

//-------------------------------------------------------------------------------
// Hocotate Toolkit を使ったディスク展開処理
//-------------------------------------------------------------------------------
internal sealed class HocotateToolkitService
{
    private static readonly TimeSpan DiscExtractTimeout = TimeSpan.FromMinutes(5);
    private readonly string _toolkitPath;

    public HocotateToolkitService(string toolkitPath)
    {
        _toolkitPath = toolkitPath;
    }

    //-------------------------------------------------------------------------------
    // ISO/GCM/GCR を検出して sys/files 展開を試行する処理
    //-------------------------------------------------------------------------------
    public bool TryExtractDiscImage(string selectedPath, out string message)
    {
        string? discPath = GetDiscImagePath(selectedPath);

        if (discPath is null)
        {
            message = "ISO/GCM/GCR が見つからないため，展開は実行していません．";
            return false;
        }

        string outputDir = Directory.Exists(selectedPath)
            ? selectedPath
            : Path.GetDirectoryName(selectedPath) ?? selectedPath;

        string[] candidates =
        {
            Path.Combine(Path.GetDirectoryName(_toolkitPath) ?? string.Empty, "resource", "DiscExtract.exe"),
            _toolkitPath
        };

        foreach (string exePath in candidates.Where(File.Exists))
        {
            if (RunExtract(exePath, discPath, outputDir, out message))
            {
                return true;
            }
        }

        message = "Hocotate Toolkit による展開を試行しましたが，extract 実行に失敗しました．";
        return false;
    }

    //-------------------------------------------------------------------------------
    // 指定パスから ISO/GCM/GCR ファイルを取得する処理
    //-------------------------------------------------------------------------------
    private static string? GetDiscImagePath(string selectedPath)
    {
        if (File.Exists(selectedPath))
        {
            string ext = Path.GetExtension(selectedPath).ToLowerInvariant();
            return ext is ".iso" or ".gcm" or ".gcr" ? selectedPath : null;
        }

        if (!Directory.Exists(selectedPath))
        {
            return null;
        }

        return Directory.EnumerateFiles(selectedPath, "*.*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path =>
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                return ext is ".iso" or ".gcm" or ".gcr";
            });
    }

    //-------------------------------------------------------------------------------
    // 展開コマンド候補を順に実行する処理
    //-------------------------------------------------------------------------------
    private static bool RunExtract(string exePath, string discPath, string outputDir, out string message)
    {
        string extension = Path.GetExtension(discPath).ToLowerInvariant();
        string[] argumentCandidates = BuildArgumentCandidates(exePath, discPath, outputDir, extension);

        foreach (string args in argumentCandidates)
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };

            try
            {
                process.Start();
                if (!process.WaitForExit((int)DiscExtractTimeout.TotalMilliseconds))
                {
                    TryKillProcess(process);
                    continue;
                }

                if (process.ExitCode == 0 &&
                    Directory.Exists(Path.Combine(outputDir, "sys")) &&
                    Directory.Exists(Path.Combine(outputDir, "files")))
                {
                    message = $"ディスク展開完了: {Path.GetFileName(exePath)}";
                    return true;
                }
            }
            catch
            {
            }
        }

        message = $"展開失敗: {Path.GetFileName(exePath)}";
        return false;
    }

    //-------------------------------------------------------------------------------
    // 実行ファイル種別に応じた展開コマンド候補を返す処理
    //-------------------------------------------------------------------------------
    private static string[] BuildArgumentCandidates(string exePath, string discPath, string outputDir, string extension)
    {
        bool isDiscExtract = string.Equals(Path.GetFileName(exePath), "DiscExtract.exe", StringComparison.OrdinalIgnoreCase);
        if (isDiscExtract)
        {
            return
            [
                $"\"{discPath}\" \"{outputDir}\"",
                $"--extract \"{discPath}\" \"{outputDir}\""
            ];
        }

        if (extension is ".iso" or ".gcm")
        {
            return
            [
                $"--gcextract \"{discPath}\" \"{outputDir}\"",
                $"--discextract \"{discPath}\" \"{outputDir}\""
            ];
        }

        return
        [
            $"--discextract \"{discPath}\" \"{outputDir}\""
        ];
    }

    //-------------------------------------------------------------------------------
    // タイムアウト時に安全にプロセス終了を試みる処理
    //-------------------------------------------------------------------------------
    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
                process.WaitForExit();
            }
        }
        catch
        {
        }
    }
}

//-------------------------------------------------------------------------------
// 指定ファイルやフォルダを再帰的に探索する処理群
//-------------------------------------------------------------------------------
internal static class RecursiveFinder
{
    //-------------------------------------------------------------------------------
    // 対象名と一致する最初のフォルダを返す処理
    //-------------------------------------------------------------------------------
    public static string? FindFirstDirectory(string root, string directoryName)
    {
        try
        {
            foreach (string dir in Directory.EnumerateDirectories(root))
            {
                if (string.Equals(Path.GetFileName(dir), directoryName, StringComparison.OrdinalIgnoreCase))
                {
                    return dir;
                }

                string? nested = FindFirstDirectory(dir, directoryName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }
        catch
        {
        }

        return null;
    }

    //-------------------------------------------------------------------------------
    // 指定ファイルを含む最初のフォルダを返す処理
    //-------------------------------------------------------------------------------
    public static string? FindDirectoryContainingFile(string root, string fileName)
    {
        try
        {
            foreach (string file in Directory.EnumerateFiles(root))
            {
                if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(file);
                }
            }

            foreach (string dir in Directory.EnumerateDirectories(root))
            {
                string? nested = FindDirectoryContainingFile(dir, fileName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
