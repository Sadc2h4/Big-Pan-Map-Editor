using System.Reflection;

namespace PikminUnitEditor;

internal static class EmbeddedImageCatalog
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    private static readonly Lazy<string[]> ResourceNames = new(() => Assembly.GetManifestResourceNames());

    //-------------------------------------------------------------------------------
    // 埋め込みリソースから画像を複製して読み込む処理
    //-------------------------------------------------------------------------------
    public static Image? LoadImage(string directoryName, string fileName)
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

        try
        {
            using Image source = Image.FromStream(stream);
            return new Bitmap(source);
        }
        catch
        {
            return null;
        }
    }

    //-------------------------------------------------------------------------------
    // アニメーションを保持したまま埋め込みリソース画像を読み込む処理
    //-------------------------------------------------------------------------------
    public static Image? LoadImageWithAnimation(string directoryName, string fileName)
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

        try
        {
            using MemoryStream buffer = new();
            stream.CopyTo(buffer);
            MemoryStream imageStream = new(buffer.ToArray());
            Image image = Image.FromStream(imageStream);
            image.Tag = imageStream;
            return image;
        }
        catch
        {
            return null;
        }
    }

    //-------------------------------------------------------------------------------
    // 埋め込みリソースからアイコンを複製して読み込む処理
    //-------------------------------------------------------------------------------
    public static Icon? LoadIcon(string directoryName, string fileName)
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

        using Icon source = new(stream);
        return (Icon)source.Clone();
    }

    //-------------------------------------------------------------------------------
    // 埋め込みリソースからストリームを開く処理
    //-------------------------------------------------------------------------------
    public static Stream? OpenStream(string directoryName, string fileName)
    {
        string? resourceName = FindResourceName(directoryName, fileName);
        return resourceName is null ? null : Assembly.GetManifestResourceStream(resourceName);
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
    // リソース名比較用にパス区切り文字を正規化する処理
    //-------------------------------------------------------------------------------
    private static string NormalizePathFragment(string value)
    {
        return value.Replace('\\', '.').Replace('/', '.').Trim('.');
    }
}
