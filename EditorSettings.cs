using System.Text.Json;

namespace PikminUnitEditor;

internal sealed class EditorSettingsStore
{
    private readonly string _settingsPath;
    private readonly string _legacySettingsPath;

    public EditorSettingsStore(string baseDirectory)
    {
        _settingsPath = Path.Combine(baseDirectory, "big_pan_map_editor.settings.json");
        _legacySettingsPath = Path.Combine(baseDirectory, "pikmin_unit_editor.settings.json");
    }

    public EditorSettings Load()
    {
        try
        {
            string path = File.Exists(_settingsPath) ? _settingsPath : _legacySettingsPath;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<EditorSettings>(json) ?? new EditorSettings();
            }
        }
        catch
        {
        }

        return new EditorSettings();
    }

    public void Save(EditorSettings settings)
    {
        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, options));
    }
}

internal sealed class EditorSettings
{
    public string ToolkitPath { get; set; } = string.Empty;
    public string DiscRoot { get; set; } = string.Empty;
    public string LastMode { get; set; } = "Field";
    public string Language { get; set; } = "ja-JP";
    public string CaveModelSource { get; set; } = "TextsGrid";
    public bool UseObjDirectView { get; set; }
}
