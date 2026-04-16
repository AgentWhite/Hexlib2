using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Manages loading and saving of application settings.
/// </summary>
public class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ASLInputTool",
        "settings.json");

    private static SettingsManager? _instance;
    private AppSettings _settings;

    /// <summary>
    /// Gets the singleton instance of the SettingsManager.
    /// </summary>
    public static SettingsManager Instance => _instance ??= new SettingsManager();

    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    public AppSettings Settings => _settings;

    /// <summary>
    /// Occurs when settings are changed.
    /// </summary>
    public event EventHandler? SettingsChanged;

    private SettingsManager()
    {
        _settings = Load();
    }

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    public void Save()
    {
        try
        {
            string directory = Path.GetDirectoryName(SettingsPath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            });
            File.WriteAllText(SettingsPath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            // Fallback: settings won't persist if write fails
        }
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception)
        {
            // Fallback to defaults
        }
        return new AppSettings();
    }
}
