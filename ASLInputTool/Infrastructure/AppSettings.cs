namespace ASLInputTool.Infrastructure;

/// <summary>
/// Represents application-wide settings for the ASL Input Tool.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the path to the folder where board data is stored.
    /// </summary>
    public string BoardsFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the folder where module data is stored.
    /// </summary>
    public string ModulesFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the folder where scenario data is stored.
    /// </summary>
    public string ScenariosFolder { get; set; } = string.Empty;
}
