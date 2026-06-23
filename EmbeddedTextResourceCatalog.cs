using System.Reflection;
using System.Text;

namespace PikminUnitEditor;

internal sealed record EmbeddedTextResource(string FileName, string Text);

internal static class EmbeddedTextResourceCatalog
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    private static readonly Lazy<string[]> ResourceNames = new(() => Assembly.GetManifestResourceNames());

    //-------------------------------------------------------------------------------
    // 埋め込みリソースから指定フォルダ内のテキストファイルを列挙して読み込む処理
    //-------------------------------------------------------------------------------
    public static IReadOnlyList<EmbeddedTextResource> LoadTextFiles(string directoryName, string extension, Encoding encoding)
    {
        string normalizedDirectory = NormalizePathFragment(directoryName);
        string marker = $".{normalizedDirectory}.";
        return ResourceNames.Value
            .Where(name => name.Contains(marker, StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            .Select(name => ReadTextResource(name, marker, encoding))
            .Where(resource => resource is not null)
            .Select(resource => resource!)
            .OrderBy(resource => resource.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    //-------------------------------------------------------------------------------
    // 埋め込みリソースから単一テキストファイルを読み込む処理
    //-------------------------------------------------------------------------------
    public static string? ReadText(string directoryName, string fileName, Encoding encoding)
    {
        string? resourceName = FindResourceName(directoryName, fileName);
        if (resourceName is null)
        {
            return null;
        }

        using Stream? stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using StreamReader reader = new(stream, encoding, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    //-------------------------------------------------------------------------------
    // 論理フォルダ名とファイル名に一致するリソース名を探す処理
    //-------------------------------------------------------------------------------
    private static string? FindResourceName(string directoryName, string fileName)
    {
        string normalizedDirectory = NormalizePathFragment(directoryName);
        string normalizedFileName = NormalizePathFragment(fileName);
        string exactSuffix = $".{normalizedDirectory}.{normalizedFileName}";

        string? exactMatch = ResourceNames.Value
            .FirstOrDefault(name => name.EndsWith(exactSuffix, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return ResourceNames.Value
            .FirstOrDefault(name => name.EndsWith($".{normalizedFileName}", StringComparison.OrdinalIgnoreCase) &&
                name.Contains($".{normalizedDirectory}.", StringComparison.OrdinalIgnoreCase));
    }

    //-------------------------------------------------------------------------------
    // リソースをテキストとして読み込み，元ファイル名を復元する処理
    //-------------------------------------------------------------------------------
    private static EmbeddedTextResource? ReadTextResource(string resourceName, string marker, Encoding encoding)
    {
        int markerIndex = resourceName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        string fileName = resourceName[(markerIndex + marker.Length)..];
        using Stream? stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using StreamReader reader = new(stream, encoding, detectEncodingFromByteOrderMarks: true);
        return new EmbeddedTextResource(fileName, reader.ReadToEnd());
    }

    //-------------------------------------------------------------------------------
    // リソース名比較用にパス区切り文字を正規化する処理
    //-------------------------------------------------------------------------------
    private static string NormalizePathFragment(string value)
    {
        return value.Replace('\\', '.').Replace('/', '.').Trim('.');
    }
}
